namespace costs.net.plugins.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.CostTemplate;
    using core.Models.Excel;
    using core.Models.User;
    using core.Services;
    using core.Services.Budget;
    using core.Services.Costs;
    using core.Services.CostTemplate;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Serilog;

    public class CostLineItemUpdater : ICostLineItemUpdater
    {
        private const string TechnicalErrorMessage = "There is an error in the Budget Form, please contact Technical Support";
        private static readonly ILogger Logger = Log.ForContext<CostLineItemUpdater>();
        private readonly ICostSectionFinder _costSectionFinder;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly ICostTemplateVersionService _costTemplateVersionService;
        private readonly ICostExchangeRateService _costExchangeRateService;

        private readonly EFContext _efContext;

        public CostLineItemUpdater(EFContext efContext,
            ICostStageRevisionService costStageRevisionService,
            ICostTemplateVersionService costTemplateVersionService,
            ICostSectionFinder costSectionFinder,
            ICostExchangeRateService costExchangeRateService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
            _costTemplateVersionService = costTemplateVersionService;
            _costSectionFinder = costSectionFinder;
            _costExchangeRateService = costExchangeRateService;
        }

        public async Task<ServiceResult<List<CostLineItem>>> Update(ExcelCellValueLookup entries, UserIdentity userIdentity, Guid costId, Guid costStageRevisionId)
        {
            if (entries == null)
            {
                Logger.Warning("Param entries is null reference. This means the uploaded budget form was not read correctly.");
                return ServiceResult<List<CostLineItem>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (userIdentity == null)
            {
                Logger.Warning("Param userIdentity is null reference.");
                return ServiceResult<List<CostLineItem>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (costId == Guid.Empty)
            {
                Logger.Warning("Param costId is Guid.Empty.");
                return ServiceResult<List<CostLineItem>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (costStageRevisionId == Guid.Empty)
            {
                Logger.Warning("Param costStageRevisionId is Guid.Empty.");
                return ServiceResult<List<CostLineItem>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (entries.Count == 0)
            {
                Logger.Warning("Param entries is empty collection. This means the uploaded budget form was not read correctly.");
                return ServiceResult<List<CostLineItem>>.CreateFailedResult(TechnicalErrorMessage);
            }

            var updatedItems = new List<CostLineItem>();
            var serviceResult = new ServiceResult<List<CostLineItem>>(updatedItems);

            var stageForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            var contentType = stageForm.GetContentType();
            var production = stageForm.GetProductionType();

            //Get the form
            var cost = await _efContext.Cost
                .Include(c => c.CostTemplateVersion)
                .ThenInclude(ctv => ctv.CostTemplate)
                .ThenInclude(ct => ct.FieldDefinitions)
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = await _efContext.CostStageRevision
                .Include(c => c.CostLineItems)
                .FirstOrDefaultAsync(c => c.Id == costStageRevisionId);
            var templateModel = await _costTemplateVersionService.GetCostTemplateVersionModel(cost.CostTemplateVersionId);
            var formResult = _costSectionFinder.GetCostSection(templateModel, contentType, production);

            if (!formResult.Success)
            {
                serviceResult.AddErrorRange(formResult.Errors);
                return serviceResult;
            }

            var form = formResult.Result;

            // SPB-3005 --> ADC-2892
            var exchangeRates = await _costExchangeRateService.GetExchangeRatesByDefaultCurrency(costId);

            //Iterate through the cost line items
            foreach (var costSection in form.CostLineItemSections)
            {
                foreach (var item in costSection.Items)
                {
                    //Build a lookup key that will look in the LookupKey column for an exact match.
                    // lookupKey format is form.costSection.item.currency or costSection.item.currency
                    var defaultLookupKey = GetDefaultCurrencyLookupKey(form, costSection, item);
                    var localLookupKey = GetLocalCurrencyLookupKey(form, costSection, item);

                    if (!entries.ContainsKey(defaultLookupKey))
                    {
                        // if the lookupKey does not exist with the form.Name included, try without the form.
                        var message = $"BF001: {form.Name}:{costSection.Name}:{item.Name} not found for cost: {costId} in uploaded budget form. Trying {costSection.Name}:{item.Name}...";
                        Logger.Warning(message);
                        defaultLookupKey = GetDefaultCurrencyLookupKey(costSection, item);
                    }

                    if (!entries.ContainsKey(localLookupKey))
                    {
                        localLookupKey = GetLocalCurrencyLookupKey(costSection, item);
                    }

                    if (entries.ContainsKey(defaultLookupKey) && entries.ContainsKey(localLookupKey))
                    {
                        var defaultCurrencyStringValue = entries[defaultLookupKey].Value;
                        var localCurrencyStringValue = entries[localLookupKey].Value;
                        var updateCell = true;
                        if (!decimal.TryParse(localCurrencyStringValue, out var localCurrencyValue))
                        {
                            var warning = $"BF002: Invalid entry '{localCurrencyStringValue}' for {costSection.Name}:{item.Name} for cost: {costId} in uploaded budget form.";
                            Logger.Warning(warning);

                            var userError = $"Invalid entry '{localCurrencyStringValue}' in cell '{entries[localLookupKey].Name}'.";
                            serviceResult.AddError(userError);
                            updateCell = false;
                        }
                        if (!decimal.TryParse(defaultCurrencyStringValue, out var defaultCurrencyValue))
                        {
                            var warning = $"BF003: Invalid entry '{defaultCurrencyStringValue}' for {costSection.Name}:{item.Name} for cost: {costId} in uploaded budget form.";
                            Logger.Warning(warning);

                            var userError = $"Invalid entry '{defaultCurrencyStringValue}' in cell '{entries[defaultLookupKey].Name}'.";
                            serviceResult.AddError(userError);
                            updateCell = false;
                        }

                        if (updateCell)
                        {
                            var cli = GetOrCreateCostLineItem(costStageRevision, userIdentity.Id, costSection, item);
                            var currency = await GetCurrency(entries, costSection, item);

                            if (HasCurrencyChanged(currency, cli) && !CanUpdateCurrency(costStageRevision))
                            {
                                var costSectionCurrency = await GetCurrencyCode(cli.LocalCurrencyId);
                                var error = new FeedbackMessage(
                                    $"The currency you have chosen in the budget form, {currency.Code} does not match the cost section currency of {costSectionCurrency}. There are two options to progress this:");
                                error.AddSuggestion("You may change the currency of your budget form to match the cost section currency and re-upload the Budget form.");
                                error.AddSuggestion(
                                    "You may cancel this datasheet, create a new cost and select the required currency. After cancelling your cost please contact P&G. They will raise and issue credit for any monies you have received against the original PO.");
                                serviceResult.AddError(error);

                                return serviceResult;
                            }

                            //Update the cost line item
                            cli.LocalCurrencyId = currency.Id;                            
                            var exchangeRate = exchangeRates.FirstOrDefault(ex => ex.FromCurrency == cli.LocalCurrencyId);
                            cli.ValueInDefaultCurrency = (exchangeRate?.Rate * localCurrencyValue) ?? 0; //we calculate it because we can't trust the sent value from front-end
                            cli.ValueInLocalCurrency = localCurrencyValue;
                            cli.SetModifiedNow();
                            updatedItems.Add(cli);
                        }
                    }
                    else
                    {
                        var userError = $"BF004: {costSection.Name}:{item.Name} not found for cost: {costId} in uploaded budget form. Adding Zero as default value.";
                        Logger.Warning(userError);
                        serviceResult.AddWarning(userError);

                        //Create a default value
                        if (item.Mandatory.HasValue && item.Mandatory.Value)
                        {
                            var cli = GetOrCreateCostLineItem(costStageRevision, userIdentity.Id, costSection, item);
                            var currency = await GetCurrency(entries, costSection, item);
                            //Update the cost line item
                            cli.LocalCurrencyId = currency.Id;
                            cli.ValueInDefaultCurrency = 0;
                            cli.ValueInLocalCurrency = 0;
                            cli.SetModifiedNow();
                            updatedItems.Add(cli);
                        }
                    }
                }
            }
            return serviceResult;
        }

        private static bool HasCurrencyChanged(Currency currency, CostLineItem cli)
        {
            return cli.LocalCurrencyId != Guid.Empty && cli.LocalCurrencyId != currency.Id;
        }

        private static bool CanUpdateCurrency(CostStageRevision costStageRevision)
        {
            return !costStageRevision.IsLineItemSectionCurrencyLocked;
        }

        private CostLineItem GetOrCreateCostLineItem(CostStageRevision costStageRevision, Guid userId, CostLineItemSectionTemplateModel section,
            CostLineItemSectionTemplateItemModel item)
        {
            if (costStageRevision.CostLineItems == null)
            {
                costStageRevision.CostLineItems = new List<CostLineItem>();
            }

            foreach (var cli in costStageRevision.CostLineItems)
            {
                if (cli.Name == item.Name &&
                    cli.CostLineItemSectionTemplate?.Name == section.Name)
                {
                    return cli;
                }
            }

            //Create new
            var costLineItem = new CostLineItem
            {
                CostStageRevision = costStageRevision,
                Id = Guid.NewGuid(),
                LocalCurrencyId = Guid.Empty,
                Name = item.Name,
                TemplateSectionId = section.Id
            };
            costStageRevision.CostLineItems.Add(costLineItem);
            costLineItem.SetCreatedNow(userId);
            return costLineItem;
        }

        private async Task<Currency> GetCurrency(ExcelCellValueLookup entries, CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            string code;
            var key = GetItemCurrencyKey(costSection, item);
            if (entries.ContainsKey(key))
            {
                code = entries[key].Value;
            }
            else
            {
                key = GetSectionCurrencyKey(costSection);
                code = entries.ContainsKey(key) ? entries[key].Value : entries[GetAgencyCurrencyKey()].Value;
            }

            return await _efContext.Currency.Where(c => c.Code == code).FirstOrDefaultAsync();
        }

        private Task<string> GetCurrencyCode(Guid currencyId)
        {
            return _efContext.Currency.Where(c => c.Id == currencyId).Select(c => c.Code).FirstOrDefaultAsync();
        }

        private static string GetDefaultCurrencyLookupKey(ProductionDetailsFormDefinitionModel form, CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            return $"{form.Name}.{costSection.Name}.{item.Name}.default";
        }

        private static string GetLocalCurrencyLookupKey(ProductionDetailsFormDefinitionModel form, CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            return $"{form.Name}.{costSection.Name}.{item.Name}.local";
        }

        private static string GetDefaultCurrencyLookupKey(CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            return $"{costSection.Name}.{item.Name}.default";
        }

        private static string GetLocalCurrencyLookupKey(CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            return $"{costSection.Name}.{item.Name}.local";
        }

        private static string GetSectionCurrencyKey(CostLineItemSectionTemplateModel costSection)
        {
            return $"currency.{costSection.Name}";
        }

        private static string GetItemCurrencyKey(CostLineItemSectionTemplateModel costSection, CostLineItemSectionTemplateItemModel item)
        {
            return $"currency.{costSection.Name}.{item.Name}";
        }

        private static string GetAgencyCurrencyKey()
        {
            return "currency.agency";
        }
    }
}

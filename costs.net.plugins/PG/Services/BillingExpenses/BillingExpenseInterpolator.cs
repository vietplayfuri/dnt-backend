
namespace costs.net.plugins.PG.Services.BillingExpenses
{
    using System;
    using System.Collections.Generic;
    using core.Builders.BillingExpenses;
    using core.Models.BillingExpenses;
    using dataAccess.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Extensions;
    using core.Models.CostTemplate;
    using core.Services.Budget;
    using core.Services.Costs;
    using core.Services.CostTemplate;
    using dataAccess;
    using Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;

    public class BillingExpenseInterpolator : IBillingExpenseInterpolator
    {
        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly ICostTemplateVersionService _costTemplateVersionService;
        private readonly ICostSectionFinder _costSectionFinder;
        private readonly ICostExchangeRateService _costExchangeRateService;

        public BillingExpenseInterpolator(
            EFContext efContext,
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

        public async Task<ICollection<CostLineItem>> InterpolateCostLineItems(
            Guid costId,
            Guid costStageRevisionId, 
            BillingExpenseData billingExpense)
        {
            /*
             *  Base Compensation Total feeds into "Base Compensation" line of Cost Section
                Pension & Health Total feeds into "Pension & Health" line of Cost Section
                Bonus Total feeds into “Bonus" line of Cost Section
                Agency Fee Total feeds into "Negotiation/broker agency fee
                Other incurred Costs Total feeds into "Other services and fees"
             */
            var interpolatedItems = new List<CostLineItem>();

            var stageForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            string contentType = stageForm.GetContentType();
            string production = stageForm.GetProductionType();

            var cost = await _efContext.Cost
                .Include(c => c.CostTemplateVersion)
                .ThenInclude(ctv => ctv.CostTemplate)
                .ThenInclude(ct => ct.FieldDefinitions)
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = await _efContext.CostStageRevision
                .Include(c => c.CostLineItems)
                .FirstOrDefaultAsync(c => c.Id == costStageRevisionId);
            var paymentCurrency = await _efContext.Currency.FirstOrDefaultAsync(c => c.Code == stageForm.AgencyCurrency);
            var templateModel = await _costTemplateVersionService.GetCostTemplateVersionModel(cost.CostTemplateVersionId);
            var form = _costSectionFinder.GetCostSection(templateModel, contentType, production).Result;
            var costLineItems = costStageRevision.CostLineItems;

            var baseCompensation = GetOrCreateCostLineItem(form, costLineItems, "baseCompensation", paymentCurrency);
            var pensionAndHealth = GetOrCreateCostLineItem(form, costLineItems, "pensionAndHealth", paymentCurrency);
            var bonusCelebrityOnly = GetOrCreateCostLineItem(form, costLineItems, "bonusCelebrityOnly", paymentCurrency);
            var negotiationBrokerAgencyFee = GetOrCreateCostLineItem(form, costLineItems, "negotiationBrokerAgencyFee", paymentCurrency);
            var otherServicesAndFees = GetOrCreateCostLineItem(form, costLineItems, "otherServicesAndFees", paymentCurrency);

            interpolatedItems.AddRange(new[] { baseCompensation, pensionAndHealth, bonusCelebrityOnly, negotiationBrokerAgencyFee, otherServicesAndFees });

            baseCompensation.ValueInLocalCurrency = GetBillingExpenseItemValue(billingExpense, Constants.BillingExpenseItem.UsageBuyoutFee);
            pensionAndHealth.ValueInLocalCurrency = GetBillingExpenseItemValue(billingExpense, Constants.BillingExpenseItem.PensionAndHealth);
            bonusCelebrityOnly.ValueInLocalCurrency = GetBillingExpenseItemValue(billingExpense, Constants.BillingExpenseItem.Bonus);
            negotiationBrokerAgencyFee.ValueInLocalCurrency = GetBillingExpenseItemValue(billingExpense, Constants.BillingExpenseItem.AgencyFee);
            otherServicesAndFees.ValueInLocalCurrency = GetBillingExpenseItemValue(billingExpense, Constants.BillingExpenseItem.OtherCosts);

            var exchangeRates = await _costExchangeRateService.GetExchangeRatesByDefaultCurrency(costStageRevision.CostStage.CostId);

            UpdateDefaultCurrency(interpolatedItems, exchangeRates);

            return interpolatedItems;
        }

        private static decimal GetBillingExpenseItemValue(BillingExpenseData billingExpense, string itemKey)
        {
            var row = billingExpense.Sections.SelectMany(s => s.Rows).FirstOrDefault(r => r.Key == itemKey);
            return row.Total;
        }

        private static CostLineItem GetOrCreateCostLineItem(ProductionDetailsFormDefinitionModel form, 
            IEnumerable<CostLineItem> costLineItems, string name, Currency paymentCurrency)
        {
            var item = costLineItems.FirstOrDefault(c => c.Name == name);

            if (item == null)
            {
                item = new CostLineItem
                {
                    Name = name,
                    LocalCurrencyId = paymentCurrency.Id
                };
                form.CostLineItemSections.ForEach(sectionTemplate =>
                {
                    var itemTemplate = sectionTemplate.Items.Find(i => i.Name == name);

                    if (itemTemplate != null)
                    {
                        item.TemplateSectionId = sectionTemplate.Id;
                    }
                });
            }
            return item;
        }

        private static void UpdateDefaultCurrency(IEnumerable<CostLineItem> costLineItems, IEnumerable<ExchangeRate> exchangeRates)
        {
            foreach (var item in costLineItems)
            {
                var exchangeRate = exchangeRates.FirstOrDefault(ex => ex.FromCurrency == item.LocalCurrencyId);
                item.UpdateDefaultCurrencyValue(exchangeRate);
            }
        }
    }
}

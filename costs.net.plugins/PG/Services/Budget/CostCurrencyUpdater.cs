namespace costs.net.plugins.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Excel;
    using core.Services;
    using core.Services.Budget;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using Serilog;

    public class CostCurrencyUpdater : ICostCurrencyUpdater
    {
        private static readonly ILogger Logger = Log.ForContext<CostLineItemUpdater>();

        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;

        private const string Production = "production";
        private const string PostProduction = "postproduction";
        private const string AgencyCurrencyMappingKey = "currency.agency";
        private const string AgencyCurrencyKey = "agency";
        private const string DirectPaymentVendorCurrencyKey = "dpv";
        private const string TechnicalErrorMessage = "There is an error in the Budget Form, please contact Technical Support";

        public CostCurrencyUpdater(EFContext efContext,
            ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<ServiceResult<Dictionary<string, Guid>>> Update(ExcelCellValueLookup entries, Guid userId, Guid costId, Guid costStageRevisionId)
        {
            if (entries == null)
            {
                Logger.Warning("Param entries is null reference. This means the uploaded budget form was not read correctly.");
                return ServiceResult<Dictionary<string, Guid>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (userId == Guid.Empty)
            {
                Logger.Warning("Param userId is Guid.Empty.");
                return ServiceResult<Dictionary<string, Guid>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (costId == Guid.Empty)
            {
                Logger.Warning("Param costId is Guid.Empty.");
                return ServiceResult<Dictionary<string, Guid>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (costStageRevisionId == Guid.Empty)
            {
                Logger.Warning("Param costStageRevisionId is Guid.Empty.");
                return ServiceResult<Dictionary<string, Guid>>.CreateFailedResult(TechnicalErrorMessage);
            }
            if (entries.Count == 0)
            {
                Logger.Warning("Param entries is empty collection. This means the uploaded budget form was not read correctly.");
                return ServiceResult<Dictionary<string, Guid>>.CreateFailedResult(TechnicalErrorMessage);
            }
            
            var stageForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            var productionForm = await _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(costStageRevisionId);
            var costStageRevision = await _efContext.CostStageRevision
                .Include(csr => csr.CostStage)
                .ThenInclude(csr => csr.CostStageRevisions)
                .FirstOrDefaultAsync(c => c.Id == costStageRevisionId);
            
            //Update the Agency Currency, if different
            var agencyCurrencyCode = GetAgencyCurrencyCode(entries);

            // return a dictionary of the currencies that have been modified i.e. agency, dpv, section.
            // Key is agency, dpv or section name and value is the currencyId. The FE has all the Ids and can update itself.
            var currencies = new Dictionary<string, Guid>();
            var serviceResult = new ServiceResult<Dictionary<string, Guid>>(currencies);
            if (stageForm.AgencyCurrency != agencyCurrencyCode)
            {
                if (CanUpdateAgencyCurrency(costStageRevision))
                {
                    var stageDetails = await _efContext.CustomFormData.FirstOrDefaultAsync(c => c.Id == costStageRevision.StageDetailsId);
                    UpdateAgencyCurrency(stageDetails, agencyCurrencyCode);
                    var agencyCurrencyId = await GetCurrencyId(agencyCurrencyCode);
                    currencies[AgencyCurrencyKey] = agencyCurrencyId;
                }
                else
                {
                    var error = new FeedbackMessage(
                        $"The agency currency you have chosen in the budget form, {agencyCurrencyCode} does not match your datasheet of {stageForm.AgencyCurrency}. There are two options to progress this:");
                    error.AddSuggestion("You may change the currency of your budget form to match the datasheet currency and re-upload the Budget form.");
                    error.AddSuggestion(
                        "You may cancel this datasheet, create a new cost and select the required agency currency. After cancelling your cost please contact P&G. They will raise and issue credit for any monies you have received against the original PO.");
                    serviceResult.AddError(error);

                    return serviceResult;
                }
            }

            // The Cost Section Currencies done by the ICostLineItemUpdater because each CostLineItem has a currency. 
            // On the UI, the Cost Section currency is set by rolling up from the first CostLineItem.

            // Here we extrapolate the currencies for the DPV and the cost line item sections in order to simplify the 
            // front end code. At this present time, the FE calculates the CLI Section currency based on the first item it finds.
            if (productionForm.DirectPaymentVendor != null)
            {
                var dpvCurrencyCode = GetDpvCurrencyCode(entries, stageForm);
                var dpvCurrencyId = await GetCurrencyId(dpvCurrencyCode);
                var productionDetails = await _efContext.CustomFormData.FirstOrDefaultAsync(c => c.Id == costStageRevision.ProductDetailsId);
                UpdateDpvCurrency(productionDetails, dpvCurrencyId);
                currencies[DirectPaymentVendorCurrencyKey] = dpvCurrencyId;
            }

            return serviceResult;
        }

        private string GetDpvCurrencyCode(ExcelCellValueLookup entries, PgStageDetailsForm stageDetails)
        {
            if (stageDetails.ProductionType?.Key == Constants.ProductionType.FullProduction)
            {
                return GetSectionCurrencyCode(entries, Production);
            }

            return GetSectionCurrencyCode(entries, PostProduction);
        }

        private static void UpdateAgencyCurrency(CustomFormData stageDetails, string agencyCurrency)
        {
            JObject stageDetailsObj = JObject.Parse(stageDetails.Data);
            stageDetailsObj["agencyCurrency"] = agencyCurrency;
            stageDetails.Data = stageDetailsObj.ToString();
        }

        private static void UpdateDpvCurrency(CustomFormData productionDetails, Guid dpvCurrencyId)
        {
            JObject productionDetailsObj = JObject.Parse(productionDetails.Data);
            productionDetailsObj["directPaymentVendor"]["currencyId"] = dpvCurrencyId;
            foreach (var node in productionDetailsObj)
            {
                // Update productionCompany or postProductionCompany or digitalDevelopmentCompany and so on...
                if (node.Key.IndexOf("company", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    node.Value["currencyId"] = dpvCurrencyId;
                }
            }
            productionDetails.Data = productionDetailsObj.ToString();
        }

        private static bool CanUpdateAgencyCurrency(CostStageRevision costStageRevision)
        {
            return !costStageRevision.IsPaymentCurrencyLocked;
        }

        private static string GetSectionCurrencyCode(ExcelCellValueLookup entries, string section)
        {
            // See excel_cell_lookups.json file for a list of currency.* mappings.
            string key = $"currency.{section.ToLower()}";

            if (entries.ContainsKey(key))
            {
                return entries[key].Value;
            }

            return string.Empty;
        }

        private static string GetAgencyCurrencyCode(ExcelCellValueLookup entries)
        {
            return entries[AgencyCurrencyMappingKey].Value;
        }

        private async Task<Guid> GetCurrencyId(string code)
        {
            return await _efContext.Currency
                .Where(c => c.Code == code)
                .Select(c => c.Id).FirstOrDefaultAsync();
        }
    }
}
namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.Currencies;
    using dataAccess;   
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;

    public class TechnicalFeeService : ITechnicalFeeService
    {
        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;        
        private readonly ICurrencyService _currencyService;
        private readonly ICostExchangeRateService _costExchangeRateService;

        public TechnicalFeeService(EFContext efContext, 
            ICostStageRevisionService costStageRevisionService,           
            ICurrencyService currencyService,
            ICostExchangeRateService costExchangeRateService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;           
            _currencyService = currencyService;
            _costExchangeRateService = costExchangeRateService;
        }

        public async Task<TechnicalFee> GetTechnicalFee(Guid costId)
        {
            var cost = await _efContext.Cost.FirstAsync(x => x.Id == costId);
            var stageDetailsForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(cost.LatestCostStageRevisionId.Value);
            //var productionDetailsForm = await _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(cost.LatestCostStageRevisionId.Value);
            var budgetRegion = stageDetailsForm.BudgetRegion?.Key;
            var contentType = stageDetailsForm.ContentType?.Key;
            var costType = cost.CostType;
            var productionType = Constants.ProductionType.ProductionTypeList.FirstOrDefault(a => a == stageDetailsForm.ProductionType?.Key);

            var queriable = _efContext.TechnicalFee.Where(x=>
                x.CostType == cost.CostType.ToString()
                && x.RegionName == budgetRegion);

            // content type is not applicable to Usage&Buyout and Trafficking/Distribution
            if (costType != CostType.Buyout && costType != CostType.Trafficking)
            {
                queriable = queriable.Where(x => x.ContentType == contentType);
            }

            if (costType != CostType.Buyout && costType != CostType.Trafficking && contentType != Constants.ContentType.Digital)
            {
                queriable = queriable.Where(x => x.ProductionType == productionType);
            }

            return await queriable.FirstOrDefaultAsync();
        }

        public async Task UpdateTechnicalFeeLineItem(Guid costId, Guid latestRevisionId)
        {
            var currentRevision = await _efContext.CostStageRevision
                            .Include(x => x.Approvals)
                                .ThenInclude(x => x.ApprovalMembers)
                                    .ThenInclude(x => x.CostUser)
                                        .ThenInclude(x=>x.UserBusinessRoles)
                                            .ThenInclude(a=>a.BusinessRole)
                            .FirstOrDefaultAsync(x => x.Id == latestRevisionId);

            var costConsultantApprovals = currentRevision.Approvals?.Where(x => 
                    x.Type == ApprovalType.IPM && x.ValidBusinessRoles?.Contains(Constants.BusinessRole.CostConsultant) == true)
                .ToList();

            var costConsultantSelected = costConsultantApprovals
                .Any(x => x.ApprovalMembers
                        .Any(m => m.CostUser.UserBusinessRoles.Any(a=>a.BusinessRole.Value == Constants.BusinessRole.CostConsultant)));
            var costLineItems = await _efContext.CostLineItem.Where(x=>x.CostStageRevisionId == latestRevisionId).ToListAsync();
            var techFeeLineItem = costLineItems.FirstOrDefault(x => x.Name == Constants.CostSection.TechnicalFee);

            if (techFeeLineItem != null)
            {
                // tech fee is applicable, let's see if we need to recalculate the value
                if (costConsultantSelected && costConsultantApprovals.Any())
                {
                    // currently there is a cost consultant selected

                    var fee = await GetTechnicalFee(costId);
                    if (fee != null && fee.ConsultantRate != 0)
                    {
                        // we got a CC rate, but we only save it if current value is 0
                        if (techFeeLineItem.ValueInDefaultCurrency == 0)
                        {
                            // calculate values based on FX rate
                            var feeCurrency = await _currencyService.GetCurrency(fee.CurrencyCode);
                            if (feeCurrency != null && !feeCurrency.DefaultCurrency)
                            {
                                // only calculate fx rate for foreign currencies                              
                                var defaultFxRate = await _costExchangeRateService.GetExchangeRateByCurrency( costId,feeCurrency.Id);
                                techFeeLineItem.ValueInDefaultCurrency = fee.ConsultantRate * defaultFxRate.Rate;
                            }
                            else
                            {
                                techFeeLineItem.ValueInDefaultCurrency = fee.ConsultantRate;
                            }

                            var localCurrency = await _currencyService.GetCurrency(techFeeLineItem.LocalCurrencyId);
                            if (localCurrency != null && !localCurrency.DefaultCurrency)
                            {                              
                                var localFxRate = await _costExchangeRateService.GetExchangeRateByCurrency(costId,techFeeLineItem.LocalCurrencyId);
                                techFeeLineItem.ValueInLocalCurrency = techFeeLineItem.ValueInDefaultCurrency / localFxRate.Rate; // reverse conversion
                            }
                            else
                            {
                                techFeeLineItem.ValueInLocalCurrency = techFeeLineItem.ValueInDefaultCurrency;
                            }
                            

                            _efContext.Update(techFeeLineItem);
                            await _efContext.SaveChangesAsync();
                        }
                    }
                }
                else if (techFeeLineItem.ValueInDefaultCurrency > 0)
                {
                    // cost consultant is not selected, but there is a fee - let's set the rate to 0 if previously we don't have any approved revisions/stages with tech fee

                    var previousRevision = await _costStageRevisionService.GetPreviousRevision(currentRevision.Id);
                    if (previousRevision != null)
                    {
                        var previousCostLineItems = await _costStageRevisionService.GetCostLineItems(previousRevision.Id);
                        var prevTechFeeLineItem = previousCostLineItems.FirstOrDefault(x => x.Name == Constants.CostSection.TechnicalFee);

                        if (prevTechFeeLineItem == null || prevTechFeeLineItem.ValueInDefaultCurrency == 0)
                        {
                            // we don't have a value - let's set it to 0
                            await SetToZeroAndSave(techFeeLineItem);
                        }
                    }
                    else
                    {
                        // we don't have a previous review - let's set it to 0
                        await SetToZeroAndSave(techFeeLineItem);
                    }
                }

            }
        }

        private async Task SetToZeroAndSave(CostLineItem techFeeLineItem)
        {
            techFeeLineItem.ValueInDefaultCurrency = 0;
            techFeeLineItem.ValueInLocalCurrency = 0;
            _efContext.Update(techFeeLineItem);
            await _efContext.SaveChangesAsync();
        }
    }
}

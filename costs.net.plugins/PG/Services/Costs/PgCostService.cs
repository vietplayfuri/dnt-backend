namespace costs.net.plugins.PG.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Response;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Exception;
    using Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Serilog;

    public class PgCostService : IPgCostService
    {
        private const string MissingVendorErrorMessage =
            "There is an error with vendor details missing for this cost. A support ticket has been raised for this. Adstream support team will be reaching out to you once this is solved";

        private readonly EFContext _efContext;
        private readonly ILogger _logger = Log.ForContext<PgCostService>();
        private readonly ICostStageRevisionService _costStageRevisionService;

        public PgCostService(EFContext efContext, ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(Guid revisionId)
        {
            var revision = await _efContext.CostStageRevision
                .Include(r => r.StageDetails)
                .Include(r => r.CostStage)
                .ThenInclude(s => s.Cost)
                .Include(r => r.CostLineItems)
                .FirstOrDefaultAsync(r => r.Id == revisionId);

            return await GetRevisionTotals(revision);
        }

        public async Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(CostStageRevision revision)
        {
            var currencyIdsForRevision = revision.CostLineItems.Select(a => a.LocalCurrencyId).ToList();
            Guid defaultPaymentCurrencyId;
            var defaultCurrency = await _efContext.Currency.Where(a => a.DefaultCurrency).Select(a => a.Id).FirstAsync();
            currencyIdsForRevision.Add(defaultCurrency);
            if (revision.CostStage.Cost.PaymentCurrencyId.HasValue)
            {
                defaultPaymentCurrencyId = revision.CostStage.Cost.PaymentCurrencyId.Value;
            }
            else
            {
                defaultPaymentCurrencyId = defaultCurrency;
            }
            currencyIdsForRevision.Add(defaultPaymentCurrencyId);
            currencyIdsForRevision = currencyIdsForRevision.Distinct().ToList();
            var currencies = await _efContext.Currency
                .Where(a => currencyIdsForRevision.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();

            var currencyExchangeRates = await _efContext.ExchangeRate
                .OrderByDescending(x => x.EffectiveFrom)
                //.AsNoTracking()
                .Where(x => currencyIdsForRevision.Contains(x.FromCurrency))
                .ToListAsync();

            var result = revision.CostLineItems.Aggregate((total: 0m, totalInLocalCurrency: 0m), (agg, cli) =>
            {
                if (cli.ValueInLocalCurrency != 0)
                {
                    var costLineItemCurrency = currencies.FirstOrDefault(a => a.Id == cli.LocalCurrencyId);

                    var exchangeRatesOnDate = currencyExchangeRates
                        .Where(x => (x.FromCurrency == costLineItemCurrency.Id || x.FromCurrency == defaultPaymentCurrencyId))
                        .OrderByDescending(x => x.EffectiveFrom)
                        .ToList();

                    if (revision.CostStage.Cost.ExchangeRateDate.HasValue) //Cost is submitted
                    {
                        exchangeRatesOnDate = exchangeRatesOnDate
                            .Where(x => (x.FromCurrency == costLineItemCurrency.Id || x.FromCurrency == defaultPaymentCurrencyId)
                                        && x.EffectiveFrom <= revision.CostStage.Cost.ExchangeRateDate.Value)
                            .OrderByDescending(x => x.EffectiveFrom)
                            .ToList();
                    }

                    var agencyExchangeRate = exchangeRatesOnDate
                        .OrderByDescending(x => x.EffectiveFrom)
                        .First(a => a.FromCurrency == defaultPaymentCurrencyId);
                    var defaultExchangeRate = exchangeRatesOnDate
                        .OrderByDescending(x => x.EffectiveFrom)
                        .First(a => a.FromCurrency == costLineItemCurrency.Id);

                    agg.totalInLocalCurrency += ((cli.ValueInLocalCurrency * defaultExchangeRate.Rate) / agencyExchangeRate.Rate);

                    agg.total += cli.ValueInDefaultCurrency;
                }
                return agg;
            });

            return result;
        }

        public async Task<OperationResponse> IsValidForSubmittion(Guid costId)
        {
            var cost = await _efContext.Cost
                .Include(c => c.Parent)
                .ThenInclude(p => p.Agency)
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(r => r.ProductDetails)
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (cost == null)
            {
                throw new EntityNotFoundException<Cost>(costId);
            }

            var productionDetails = _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(cost.LatestCostStageRevision);
            var isDpv = productionDetails?.DirectPaymentVendor != null;
            var sapCode = isDpv
                ? productionDetails.DirectPaymentVendor.SapVendorCode
                : cost.Parent.Agency.Labels.GetSapVendorCode();

            var isValid = !string.IsNullOrWhiteSpace(sapCode);
            var errorMessage = isValid ? string.Empty : MissingVendorErrorMessage;

            return new OperationResponse(isValid, errorMessage);
        }
    }
}

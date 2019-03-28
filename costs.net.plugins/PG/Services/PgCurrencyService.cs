using System.Threading.Tasks;

namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Linq;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Exception;
    using Form;
    using Microsoft.EntityFrameworkCore;

    public class PgCurrencyService : IPgCurrencyService
    {
        private readonly EFContext _efContext;
        public PgCurrencyService(EFContext efContext)
        {
            _efContext = efContext;
        }

        public Task<Currency> GetCurrencyIfChanged(PgStageDetailsForm oldStageDetails, PgProductionDetailsForm oldProductionDetails, PgStageDetailsForm newStageDetails, PgProductionDetailsForm newProductionDetails)
        {
            if ((newProductionDetails?.DirectPaymentVendor?.CurrencyId != oldProductionDetails?.DirectPaymentVendor?.CurrencyId)
                || (newStageDetails.AgencyCurrency != oldStageDetails.AgencyCurrency))
            {
                return GetCurrency(newStageDetails.AgencyCurrency, newProductionDetails);
            }

            return Task.FromResult<Currency>(null);
        }

        public async Task<string> GetCurrencyCode(string agencyCurrency, PgProductionDetailsForm productionDetails)
        {
            var currency = await GetCurrency(agencyCurrency, productionDetails);
            return currency.Code;
        }

        public async Task<Currency> GetCurrency(string agencyCurrency, PgProductionDetailsForm productionDetails)
        {
            Currency currency;
            if (productionDetails?.DirectPaymentVendor != null)
            {
                var dpv = productionDetails.DirectPaymentVendor;
                if (dpv.CurrencyId.HasValue)
                {
                    currency = await _efContext.Currency.FirstOrDefaultAsync(x => x.Id == productionDetails.DirectPaymentVendor.CurrencyId.Value);
                    if (currency == null)
                    {
                        throw new EntityNotFoundException<Entity>(dpv.CurrencyId.Value);
                    }
                }
                else
                {
                    var vendor = await _efContext.Vendor
                        .Include(v => v.Categories)
                        .FirstOrDefaultAsync(x => x.Id == dpv.Id);

                    if (vendor == null)
                    {
                        throw new EntityNotFoundException<Vendor>(dpv.Id);
                    }

                    var vendorCategory = vendor.Categories.FirstOrDefault(c => c.Name == dpv.ProductionCategory);
                    if (vendorCategory == null)
                    {
                        throw new Exception($"Category {dpv.ProductionCategory} is not enabled for vendor {vendor.Name} id {vendor.Id}");
                    }

                    currency = await _efContext.Currency.FirstOrDefaultAsync(x => x.Id == vendorCategory.DefaultCurrencyId);
                }
            }
            else if (!string.IsNullOrEmpty(agencyCurrency))
            {
                currency = await _efContext.Currency.FirstOrDefaultAsync(x => x.Code == agencyCurrency);
            }
            else
            {
                currency = await _efContext.Currency.FirstOrDefaultAsync(x => x.DefaultCurrency);
            }

            return currency;
        }
    }
}

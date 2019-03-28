namespace costs.net.plugins.PG.Services
{
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using Form;

    public interface IPgCurrencyService
    {
        Task<Currency> GetCurrencyIfChanged(PgStageDetailsForm oldStageDetails, PgProductionDetailsForm oldProductionDetails, PgStageDetailsForm newStageDetails, PgProductionDetailsForm newProductionDetails);
        Task<string> GetCurrencyCode(string agencyCurrency, PgProductionDetailsForm produtionDetails);
        Task<Currency> GetCurrency(string agencyCurrency, PgProductionDetailsForm produtionDetails);
    }
}
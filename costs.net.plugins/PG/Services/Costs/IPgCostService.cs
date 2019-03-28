namespace costs.net.plugins.PG.Services.Costs
{
    using System;
    using System.Threading.Tasks;
    using core.Models.Response;
    using dataAccess.Entity;

    public interface IPgCostService
    {
        Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(Guid revisionId);

        Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(CostStageRevision revision);

        Task<OperationResponse> IsValidForSubmittion(Guid costId);
    }
}

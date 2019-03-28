namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Threading.Tasks;
    using core.Models.Payments;
    using dataAccess.Entity;

    public interface IPgPaymentService
    {
        Task<PaymentAmountResult> GetPaymentAmount(Guid costStageRevisionId, bool persist = true);

        Task<PaymentAmountResult> GetPaymentAmount(CostStageRevision costStageRevision, bool persist = true);

        Task<Currency> GetPaymentCurrency(Guid costStageRevisionId);
    }
}

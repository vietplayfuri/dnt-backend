namespace costs.net.plugins.PG.Builders.Payments
{
    using System.Collections.Generic;
    using dataAccess.Entity;
    using Models.Payments;

    public interface IPgCostStageRevisionTotalPaymentsBuilder
    {
        CostStageRevisionTotalPayments Build(List<CostStageRevisionPaymentTotal> payments);
    }
}

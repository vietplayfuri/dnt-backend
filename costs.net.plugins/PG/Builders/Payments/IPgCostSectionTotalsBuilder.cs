namespace costs.net.plugins.PG.Builders.Payments
{
    using System.Collections.Generic;
    using dataAccess.Views;
    using Form;
    using Models.Payments;

    public interface IPgCostSectionTotalsBuilder
    {
        CostSectionTotals Build(PgStageDetailsForm stage, IEnumerable<CostLineItemView> costLineItems, string currentStageKey);
    }
}

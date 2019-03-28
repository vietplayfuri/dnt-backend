namespace costs.net.plugins.PG.Services.PurchaseOrder
{
    using System;

    public class PgPurchaseOrderDTO
    {
        public string BrandName { get; set; }

        public string CostNumber { get; set; }

        public string StageDetailsData { get; set; }

        public string ProductionDetailsData { get; set; }

        public Guid LatestCostStageRevisionId { get; set; }

        public string[] AgencyLabels { get; set; }

        public string CostStageRevisionKey { get; set; }

        public string CostStageRevisionName { get; set; }

        public string RequisitionerEmail { get; set; }

        public string TNumber { get; set; }
    }
}
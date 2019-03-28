namespace costs.net.plugins.PG.Models.PurchaseOrder
{
    using costs.net.dataAccess.Entity;
    using System;
    using System.Collections.Generic;

    public class PgPurchaseOrder
    {
        public class LongTextField
        {
            public LongTextField()
            {
                VN = new List<string>();
                AN = new List<string>();
                BN = new List<string>();
            }

            public List<string> VN { get; set; }

            public List<string> AN { get; set; }

            public List<string> BN { get; set; }
        }

        public string BasketName { get; set; }

        public string Description { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public string Currency { get; set; }

        public string Vendor { get; set; }

        public string CategoryId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string CostNumber { get; set; }

        public string GL { get; set; }

        public string IONumber { get; set; }

        public string TNumber { get; set; }

        public string RequisitionerEmail { get; set; }

        public LongTextField LongText { get; set; }

        public string PoNumber { get; set; }

        public string AccountCode { get; set; }

        public string ItemIdCode { get; set; }

        public string[] GrNumbers { get; set; }

        public string Commodity { get; set; }
    }

    /// <summary>
    /// ADC-2845
    /// </summary>
    public class XMGOrder : PgPurchaseOrder
    {
        public string StageName { get; set; }
        public CostStageRevisionStatus Status { get; set; }

        /// <summary>
        /// Created date of Cost
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Modified date of Revision
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// custom_object_data > "name" = 'PaymentDetails' and json_extract_path_text("data"::json, 'requisition')
        /// </summary>
        public string RequisitionId { get; set; }

        /// <summary>
        /// Accumulated amount already raised
        /// </summary>
        public decimal? AccumulatedAmount { get; set; }

        public List<XMGPaidStep> PaidSteps { get; set; }
    }

    public class XMGPaidStep
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}

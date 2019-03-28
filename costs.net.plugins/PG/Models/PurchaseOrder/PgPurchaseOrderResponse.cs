namespace costs.net.plugins.PG.Models.PurchaseOrder
{
    using System;

    public class PgPurchaseOrderResponse
    {
        /// <summary>
        ///     Requisition number
        /// </summary>
        public string Requisition { get; set; }

        /// <summary>
        ///     Purchase order number
        /// </summary>
        public string PoNumber { get; set; }

        /// <summary>
        ///     Email of apprrover
        /// </summary>
        public string ApproverEmail { get; set; }

        /// <summary>
        ///     Email address of IO# owner
        /// </summary>
        public string IoNumberOwner { get; set; }

        /// <summary>
        ///     Goods receipt number
        /// </summary>
        public string GrNumber { get; set; }

        /// <summary>
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        ///     Purchase order date
        /// </summary>
        public DateTime? PoDate { get; set; }

        /// <summary>
        ///     Goods receipt date
        /// </summary>
        public DateTime? GrDate { get; set; }

        /// <summary>
        ///     Full account code
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        ///     comments
        /// </summary>
        public string Comments { get; set; }

        public string Type { get; set; }

        /// <summary>
        ///     Item ID Code (Item Code ID)
        /// </summary>
        public string ItemIdCode { get; set; }

        /// <summary>
        ///     "Approved", "Rejected", "Awaiting"
        /// </summary>
        public string ApprovalStatus { get; set; }

        /// <summary>
        ///     Total amount for which XMG received update from Coupa
        /// </summary>
        /// <value>
        ///     The total cost amount.
        /// </value>
        public decimal? TotalAmount { get; set; }
    }
}

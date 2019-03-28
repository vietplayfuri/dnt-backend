namespace costs.net.plugins.PG.Models
{
    using System;

    public class PgPaymentDetails
    {
        public string PoNumber { get; set; }

        public string IoNumber { get; set; }

        public string IoNumberOwner { get; set; }

        public string GrNumber { get; set; }

        public string GlAccount { get; set; }

        public string Requisition { get; set; }

        public DateTime? FinalAssetApprovalDate { get; set; }

    }
}

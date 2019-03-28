namespace costs.net.plugins.PG.Models.Rules
{
    public class PgActionRule
    {
        public bool IsOwner { get; set; }

        public bool IsApprover { get; set; }

        public string CostStage { get; set; }

        public string Status { get; set; }

        public bool IsRevision { get; set; }

        public bool HasPONumber { get; set; }

        public bool NeverSubmitted { get; set; }

        public bool HasExternalIntegration { get; set; }

        public bool CostTotalBelowAuthLimit { get; set; }

        public decimal CostStageTotal { get; set; }

        public bool IsAdmin { get; set; }

        public bool UserIsIPMAndApproved { get; set; }

        public bool UserIsFinanceManager { get; set; }

        public bool IsLatestRevision { get; set; }
    }
}
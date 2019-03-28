namespace costs.net.plugins.PG.Models.Rules
{
    public class PgStatusRule
    {
        public string Status { get; set; }

        public string BudgetRegion { get; set; }

        public bool IsCyclone { get; set; }

        public string Action { get; set; }

        public string CostType { get; set; }

        public bool HasTechnicalApproval { get; set; }

        public bool HasBrandApproval { get; set; }
        public string CostStage { get; set; }
    }
}
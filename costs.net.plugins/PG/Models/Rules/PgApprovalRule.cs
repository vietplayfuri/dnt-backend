namespace costs.net.plugins.PG.Models.Rules
{
    public class PgApprovalRule
    {
        public string ContentType { get; set; }

        public string ProductionType { get; set; }

        public string BudgetRegion { get; set; }

        public string CostType { get; set; }

        public decimal TotalCostAmount { get; set; }

        public bool IsCyclone { get; set; }
    }
}
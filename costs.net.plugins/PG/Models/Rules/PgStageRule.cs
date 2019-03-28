namespace costs.net.plugins.PG.Models.Rules
{
    public class PgStageRule
    {
        public string ContentType { get; set; }

        public string ProductionType { get; set; }

        public string BudgetRegion { get; set; }

        public string CostType { get; set; }

        public decimal TargetBudgetAmount { get; set; }

        public bool IsAIPE { get; set; }

        public decimal TotalCostAmount { get; set; }
    }
}
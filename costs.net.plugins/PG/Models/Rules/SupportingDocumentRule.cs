
namespace costs.net.plugins.PG.Models.Rules
{
    public class SupportingDocumentRule
    {
        public string BudgetRegion { get; set; }

        public string CostStage { get; set; }

        public string ProductionType { get; set; }

        public string ContentType { get; set; }

        public string CostType { get; set; }

        public bool Mandatory { get; set; }

        public bool TotalCostIncreased { get; set; }

        public string PreviousCostStage { get; set; }
    }
}

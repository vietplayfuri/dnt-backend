namespace costs.net.plugins.PG.Models.Payments
{    
    public class CostSectionTotals
    {
        public decimal TotalCostAmountTotal { get; set; }
        public decimal ProductionCostTotal { get; set; }
        public decimal InsuranceCostTotal { get; set; }
        public decimal TechnicalFeeCostTotal { get; set; }
        public decimal TalentFeeCostTotal { get; set; }
        public decimal PostProductionCostTotal { get; set; }
        public decimal TargetBudgetTotal { get; set; }
        public decimal OtherCostTotal { get; set; }

        public override string ToString()
        {
            return $"CostSectionTotals: {TotalCostAmountTotal}:{ProductionCostTotal}:{InsuranceCostTotal}" +
                   $":{TechnicalFeeCostTotal}:{TalentFeeCostTotal}:{PostProductionCostTotal}:{TargetBudgetTotal}" +
                   $":{OtherCostTotal}";
        }
    }
}

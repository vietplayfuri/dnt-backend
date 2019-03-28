namespace costs.net.plugins.PG.Models.Payments
{    
    public class CostStageRevisionTotalPayments
    {
        public decimal ProductionCostPayments { get; set; }
        public decimal InsuranceCostPayments { get; set; }
        public decimal TechnicalFeeCostPayments { get; set; }
        public decimal TalentFeeCostPayments { get; set; }
        public decimal PostProductionCostPayments { get; set; }
        public decimal TargetBudgetTotalCostPayments { get; set; }
        public decimal OtherCostPayments { get; set; }
        public decimal TotalCostPayments { get; set; }

        public decimal CarryOverAmount { get; set; }
    }
}
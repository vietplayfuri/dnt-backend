namespace costs.net.plugins.PG.Models.Rules
{
    using System;
    using Payments;

    public class PgPaymentRule
    {
        public string ProductionType { get; set; }

        public string BudgetRegion { get; set; }

        public string ContentType { get; set; }

        public string CostType { get; set; }

        public string CostStages { get; set; }

        public Guid? DirectPaymentVendorId { get; set; }

        public bool IsAIPE { get; set; }

        public decimal TotalCostAmount { get; set; }

        public decimal? ProductionCost { get; set; }
        public decimal? InsuranceCost { get; set; }
        public decimal? TechnicalFeeCost { get; set; }
        public decimal? TalentFeeCost { get; set; }
        public decimal? PostProductionCost { get; set; }
        public decimal? OtherCost { get; set; }
        public decimal? TargetBudgetTotalCost { get; set; }
        public decimal? TotalCost { get; set; }

        public CostSectionTotals StageTotals { get; set; }
        public decimal? CostCarryOverAmount { get; set; }

        public override string ToString()
        {
            return $"PgPaymentRule: {ProductionType}:{BudgetRegion}:{ContentType}:{CostType}:{CostStages}" +
                   $":{DirectPaymentVendorId}:{IsAIPE}:{TotalCostAmount}:{ProductionCost}:{InsuranceCost}" +
                   $":{TechnicalFeeCost}:{TalentFeeCost}:{PostProductionCost}:{OtherCost}:{TargetBudgetTotalCost}" +
                   $":{TotalCost}:{TotalCost}:{CostCarryOverAmount}:{StageTotals}";
        }
    }
}

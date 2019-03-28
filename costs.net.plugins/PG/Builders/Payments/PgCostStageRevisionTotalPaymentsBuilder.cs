
namespace costs.net.plugins.PG.Builders.Payments
{
    using System.Collections.Generic;
    using System.Linq;
    using dataAccess.Entity;
    using Models.Payments;

    public class PgCostStageRevisionTotalPaymentsBuilder : IPgCostStageRevisionTotalPaymentsBuilder
    {
        public CostStageRevisionTotalPayments Build(List<CostStageRevisionPaymentTotal> payments)
        {
            var totalPayments = new CostStageRevisionTotalPayments();
            if (payments != null)
            {
                totalPayments.InsuranceCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.InsuranceTotal).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.TechnicalFeeCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.TechnicalFee).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.TalentFeeCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.TalentFees).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.PostProductionCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.PostProduction).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.ProductionCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.Production).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.TargetBudgetTotalCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.TargetBudgetTotal).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();
                totalPayments.OtherCostPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.Other).Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();

                var costTotalPayments = payments.Where(x => x.LineItemTotalType == Constants.CostSection.CostTotal);
                totalPayments.TotalCostPayments = costTotalPayments.Select(x => x.LineItemTotalCalculatedValue).DefaultIfEmpty(0).Sum();

                // it is only relevant for the last revision
                totalPayments.CarryOverAmount = costTotalPayments.OrderByDescending(x => x.CalculatedAt).Select(x => x.LineItemRemainingCost).DefaultIfEmpty(0).First();
            }
            else
            {
                totalPayments.ProductionCostPayments = 0;
                totalPayments.InsuranceCostPayments = 0;
                totalPayments.TechnicalFeeCostPayments = 0;
                totalPayments.TalentFeeCostPayments = 0;
                totalPayments.PostProductionCostPayments = 0;
                totalPayments.OtherCostPayments = 0;
                totalPayments.TargetBudgetTotalCostPayments = 0;
                totalPayments.TotalCostPayments = 0;
            }

            return totalPayments;
        }
    }
}

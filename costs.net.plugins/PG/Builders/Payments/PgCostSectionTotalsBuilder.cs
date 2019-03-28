
namespace costs.net.plugins.PG.Builders.Payments
{
    using System.Collections.Generic;
    using System.Linq;
    using dataAccess.Views;
    using Form;
    using Models.Payments;
    using Models.Stage;
    using Serilog;

    public class PgCostSectionTotalsBuilder : IPgCostSectionTotalsBuilder
    {
        private static readonly ILogger Logger = Log.ForContext<PgCostSectionTotalsBuilder>();

        // this is all calculated in Default Currency (USD)
        // to be later converted into Agency Currency
        public CostSectionTotals Build(PgStageDetailsForm stage, IEnumerable<CostLineItemView> costLineItems, string currentStageKey)
        {
            var totals = new CostSectionTotals();

            if (stage == null)
            {
                Logger.Warning($"PY001: {nameof(stage)} is null.");
            }

            if (costLineItems == null)
            {
                Logger.Error($"PY002: {nameof(costLineItems)} is null.");
                return totals;
            }

            if (string.IsNullOrEmpty(currentStageKey))
            {
                Logger.Error($"PY003: {nameof(currentStageKey)} is null or empty string.");
                return totals;
            }

            // NB: individual items are found by their name
            var costLineItemsArray = costLineItems as CostLineItemView[] ?? costLineItems.ToArray();

            var productionInsurance = costLineItemsArray.Where(x => x.Name == Constants.CostSection.ProductionInsurance).Sum(x => x.ValueInDefaultCurrency);
            var postProductionInsurance = costLineItemsArray.Where(x => x.Name == Constants.CostSection.PostProductionInsurance).Sum(x => x.ValueInDefaultCurrency);

            totals.InsuranceCostTotal = productionInsurance + postProductionInsurance;

            totals.TechnicalFeeCostTotal = costLineItemsArray.Where(x => x.Name == Constants.CostSection.TechnicalFee).Sum(x => x.ValueInDefaultCurrency);
            totals.TalentFeeCostTotal = costLineItemsArray.Where(x => x.Name == Constants.CostSection.TalentFees).Sum(x => x.ValueInDefaultCurrency);

            // NB: composite items are found by template section name
            totals.PostProductionCostTotal = costLineItemsArray.Where(x => x.TemplateSectionName == Constants.CostSection.PostProduction).Sum(x => x.ValueInDefaultCurrency) - postProductionInsurance;
            totals.ProductionCostTotal = costLineItemsArray.Where(x => x.TemplateSectionName == Constants.CostSection.Production).Sum(x => x.ValueInDefaultCurrency) - productionInsurance;

            // target budget total is relevant only for AIPE stage
            if (currentStageKey == CostStages.Aipe.ToString())
            {
                totals.TargetBudgetTotal = stage.InitialBudget.GetValueOrDefault();
                totals.TotalCostAmountTotal = totals.TargetBudgetTotal;
                totals.OtherCostTotal = 0;
            }
            else
            {
                totals.TotalCostAmountTotal = costLineItemsArray.Sum(i => i.ValueInDefaultCurrency);
                totals.OtherCostTotal = totals.TotalCostAmountTotal - totals.InsuranceCostTotal - totals.TechnicalFeeCostTotal - totals.PostProductionCostTotal - totals.ProductionCostTotal;
            }

            return totals;
        }
    }
}

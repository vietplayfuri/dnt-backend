namespace costs.net.plugins.PG.Models.Rules
{
    using System.Linq;
    using Stage;

    public class PgPaymentRuleDefinition
    {
        public bool DetailedSplit { get; set; }
        public PgPaymentRuleDefinitionSplit[] Splits { get; set; }

        public decimal? GetSplitByNameAndStage(string name, CostStages stage)
        {
            var split = GetByNameAndStage(name, stage);
            return split;
        }

        public bool HasExplicitSplitForSectionAtStage(string sectionName, CostStages costStage)
        {
            var split = GetByNameAndStage(sectionName, costStage);

            return split.HasValue;
        }

        private decimal? GetByNameAndStage(string name, CostStages stage)
        {
            var split = Splits.FirstOrDefault(x => x.CostTotalName == name);
            if (split != null)
            {
                switch (stage)
                {
                    case CostStages.Aipe:
                        return split.AIPESplit;
                    case CostStages.OriginalEstimate:
                        return split.OESplit;
                    case CostStages.FirstPresentation:
                        return split.FPSplit;
                    case CostStages.FinalActual:
                    case CostStages.FinalActualRevision:
                        return split.FASplit;
                    default:
                        return 0;
                }
            }

            return null;
        }
    }
}
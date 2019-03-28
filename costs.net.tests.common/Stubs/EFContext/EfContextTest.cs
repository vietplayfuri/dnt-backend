namespace costs.net.tests.common.Stubs.EFContext
{
    using System.Linq;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using plugins.PG.Form;

    public class EFContextTest : EFContext
    {
        public EFContextTest()
        {
        }

        public EFContextTest(EFContextOptions options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {
        }

        public override IQueryable<Cost> GetCostsByStageDetailsFieldValue(string[] jsonPath, string[] values)
        {
            return Cost
                .Where(c => 
                    c.LatestCostStageRevision != null 
                    && c.LatestCostStageRevision.StageDetails != null
                    && MatchField(c, jsonPath, values)
                    );
        }
        
        private static bool MatchField(Cost cost, string[] jsonPath, string[] values)
        {
            if (jsonPath == null || jsonPath.Length == 0)
            {
                return false;
            }

            var stageDetails = JsonConvert.DeserializeObject<PgStageDetailsForm>(cost.LatestCostStageRevision.StageDetails.Data);
            if (stageDetails == null)
            {
                return false;
            }

            switch (jsonPath[0])
            {
                case nameof(PgStageDetailsForm.SmoName):
                    return values.Any(v => v == stageDetails.SmoName);
                case nameof(PgStageDetailsForm.BudgetRegion):
                    return values.Any(v => v == stageDetails.BudgetRegion.Name);
            }
            return false;
        }
    }
}
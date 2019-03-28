namespace costs.net.plugins.PG.Services.Rule
{
    using System;
    using System.Threading.Tasks;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Models.Stage;

    public class PgRuleService : IPluginRuleService
    {
        private readonly EFContext _efContext;

        public PgRuleService(EFContext efContext)
        {
            _efContext = efContext;
        }

        public async Task<bool> CanEditIONumber(Guid costId)
        {
            var cost = await _efContext.Cost
                .Include(c => c.CostStages)
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .FirstOrDefaultAsync(c => c.Id == costId);

            // Enabled only on First Stage, ex for AIPE is Aipe and for remaining is OE
            return cost != null && IsCostStageValid(cost.LatestCostStageRevision.CostStage.Key, cost.CostType)
                                   && cost.CostStages.Count == 1     //making sure is first stage
                                   && cost.Status == CostStageRevisionStatus.Draft;
        }

        private static bool IsCostStageValid(string costStageKey, CostType costType)
        {
            return (costStageKey == CostStages.OriginalEstimate.ToString() || costStageKey == CostStages.Aipe.ToString())
                        || ((costType == CostType.Buyout || costType == CostType.Trafficking) && costStageKey == CostStages.FinalActual.ToString());
        }
    }
}
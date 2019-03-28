namespace costs.net.plugins.PG.Builders.Workflow
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Workflow;
    using core.Models.Workflow;
    using core.Services.Costs;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using Models.Rules;
    using Newtonsoft.Json;

    public class PgCostStatusResolver : ICostStatusResolver
    {
        private readonly EFContext _efContext;
        private readonly ILogger _logger;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly IRuleService _ruleService;

        public PgCostStatusResolver(
            EFContext efContext, 
            ILogger logger, 
            ICostStageRevisionService costStageRevisionService, 
            IRuleService ruleService)
        {
            _efContext = efContext;
            _logger = logger;
            _costStageRevisionService = costStageRevisionService;
            _ruleService = ruleService;
        }

        public async Task<CostStageRevisionStatus> GetNextStatus(Guid costId, CostAction action)
        {
            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr => csr.Approvals)
                        .ThenInclude(a => a.ApprovalMembers)
                .Include(c => c.Project)
                .Include(c => c.Parent)
                    .ThenInclude(p => p.Agency)
                .FirstOrDefaultAsync(c => c.Id == costId);

            _logger.Information($"Working out next status for the cost {cost.CostNumber} {cost.Id}. Current status: {cost.Status} Action: {action}");

            var stageDetails = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(cost.LatestCostStageRevision.Id);

            var rules = await _ruleService.GetCompiledByRuleType<PgStatusRule>(RuleType.Status);
            var testRule = new PgStatusRule
            {
                Status = cost.LatestCostStageRevision.Status.ToString(),
                BudgetRegion = stageDetails.BudgetRegion?.Key,
                Action = action.ToString(),
                CostType= cost.CostType.ToString(),
                IsCyclone = cost.Parent.Agency.IsCyclone(),
                HasTechnicalApproval = cost.LatestCostStageRevision.Approvals.Any(a => a.Type == ApprovalType.IPM),
                HasBrandApproval = cost.LatestCostStageRevision.Approvals.Any(a => a.Type == ApprovalType.Brand),
                CostStage = cost.LatestCostStageRevision.Name
            };

            if (_ruleService.TryMatchRule(rules, testRule, (r, dr) => JsonConvert.DeserializeObject<PgStatusRuleDefinition>(dr.Definition).Status, out var status))
            {
                _logger.Information($"Next cost status for cost {cost.CostNumber} is {status}. Previous status {cost.Status}");
                return status;
            }

            throw new Exception($"Couldn't find status transition for cost {cost.CostNumber} rule: {JsonConvert.SerializeObject(testRule)}");
        }
    }
}
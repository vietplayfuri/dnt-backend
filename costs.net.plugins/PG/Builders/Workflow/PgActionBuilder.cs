namespace costs.net.plugins.PG.Builders.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Workflow;
    using core.Models.ACL;
    using core.Models.Rule;
    using core.Models.User;
    using core.Models.Workflow;
    using core.Services.CustomData;
    using core.Services.Rules;
    using costs.net.plugins.PG.Form;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Models.PurchaseOrder;
    using Models.Rules;
    using Models.Stage;
    using Newtonsoft.Json;
    using Services;
    using Services.Role;
    using static costs.net.core.Constants;

    public class PgActionBuilder : IActionBuilder
    {
        private static readonly string[] RevisionStages = {
            CostStages.OriginalEstimateRevision.ToString(),
            CostStages.FirstPresentationRevision.ToString()
        };

        private readonly EFContext _efContext;
        private readonly IRuleService _ruleService;
        private readonly ICustomObjectDataService _customObjectDataService;
        private readonly IPgCostUserRoleService _costUserRoleService;

        public PgActionBuilder(
            EFContext efContext,
            IRuleService ruleService,
            ICustomObjectDataService customObjectDataService,
            IPgCostUserRoleService costUserRoleService
            )
        {
            _efContext = efContext;
            _ruleService = ruleService;
            _customObjectDataService = customObjectDataService;
            _costUserRoleService = costUserRoleService;
        }

        public async Task<Dictionary<string, ActionModel>> GetActions(Guid costId, UserIdentity userIdentity)
        {
            var costActions = await GetCostActions(new[] { costId }, userIdentity);
            return costActions[costId].ActionModels;
        }

        public async Task<Dictionary<string, ActionModel>> GetActionsByCostStageRevision(Guid costStageRevisionId, UserIdentity userIdentity)
        {
            //Pre - load all needed data into EFContext
            var revision = await _efContext.CostStageRevision
                .Include(csr => csr.CostStage).ThenInclude(cs => cs.Cost)//.ThenInclude(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions)
                .Include(csr => csr.CustomObjectData)
                .Include(csr => csr.Approvals).ThenInclude(a => a.ApprovalMembers)
                .Include(csr => csr.StageDetails)
                .Include(csr => csr.CostLineItems)
                //.Include(csr => csr.CostStageRevisionPaymentTotals)
                .FirstOrDefaultAsync(csr => csr.Id == costStageRevisionId);

            var cost = revision.CostStage.Cost;

            var costUser = await _efContext.CostUser
                .Include(cu => cu.UserUserGroups).ThenInclude(uug => uug.UserGroup).ThenInclude(ug => ug.Role)
                .Include(cu => cu.UserBusinessRoles).ThenInclude(ubr => ubr.BusinessRole)
                .Where(x => x.Id == userIdentity.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var actions = await GetCostActions(userIdentity, cost, revision, costUser);

            return actions;
        }

        public async Task<Dictionary<Guid, CostActionsModel>> GetActions(IEnumerable<Guid> costIds, UserIdentity userIdentity)
        {
            var ids = costIds as Guid[] ?? costIds.ToArray();
            return await GetCostActions(ids, userIdentity);
        }

        private async Task<Dictionary<Guid, CostActionsModel>> GetCostActions(Guid[] costIds, UserIdentity userIdentity)
        {
            var result = new Dictionary<Guid, CostActionsModel>();

            // Pre-load all needed data into EFContext
            var costs = await _efContext.Cost
                //.Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions)
                .Include(c => c.LatestCostStageRevision).ThenInclude(csr => csr.CostStage)
                .Include(c => c.LatestCostStageRevision).ThenInclude(csr => csr.CustomObjectData)
                .Include(c => c.LatestCostStageRevision).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.ApprovalMembers)
                .Include(c => c.LatestCostStageRevision).ThenInclude(csr => csr.StageDetails)
                .Include(c => c.LatestCostStageRevision).ThenInclude(csr => csr.CostLineItems)
                .Where(c => costIds.Contains(c.Id))
                .AsNoTracking()
                .ToListAsync();
            
            var costUser = await _efContext.CostUser
                .Include(cu => cu.UserUserGroups).ThenInclude(uug => uug.UserGroup).ThenInclude(ug => ug.Role)
                .Include(cu => cu.UserBusinessRoles).ThenInclude(ubr => ubr.BusinessRole)
                .Where(x => x.Id == userIdentity.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            foreach (var cost in costs)
            {
                var actions = await GetCostActions(userIdentity, cost, cost.LatestCostStageRevision, costUser);
                result.Add(cost.Id, new CostActionsModel(cost, actions));
            }

            return result;
        }

        private async Task<Dictionary<string, ActionModel>> GetCostActions(UserIdentity userIdentity,
            Cost cost, CostStageRevision costStageRevision, CostUser user)
        {
            var isCostNeverSubmitted = costStageRevision.Status == CostStageRevisionStatus.Draft && costStageRevision.Name == CostStageConstants.OriginalEstimate;

            var stageDetails = JsonConvert.DeserializeObject<PgStageDetailsForm>(costStageRevision.StageDetails?.Data);
            var costUser = new
            {
                isApprover = user.UserUserGroups.Any(uug => uug.UserGroup.ObjectId == cost.Id && uug.UserGroup.Role.Name == Roles.CostApprover),
                isAdmin = user.UserUserGroups.Any(x => x.UserGroup.Role.Name == Roles.ClientAdmin && x.UserGroup.ObjectId == userIdentity.ModuleId),
                authLimit = user.ApprovalLimit,
                isFinanceManager = user.UserBusinessRoles.Any(ubr => ubr.BusinessRole != null && ubr.BusinessRole.Key == Constants.BusinessRole.FinanceManager && (ubr.ObjectId != null || ubr.Labels.Contains(stageDetails.SmoName)))
            };

            var purchaseOrderResponse = _customObjectDataService
                .GetCustomData<PgPurchaseOrderResponse>(costStageRevision.CustomObjectData, CustomObjectDataKeys.PgPurchaseOrderResponse);

            // current user is IPM user and is approved
            var userIsIPMAndApproved = costStageRevision.Approvals
                .Any(s => s.ApprovalMembers.Any(a => a.MemberId == userIdentity.Id && !a.IsExternal && a.Status == ApprovalStatus.Approved));

            var isLatestRevision = cost.LatestCostStageRevisionId == costStageRevision.Id;

            var paymentBelowAuthLimit = true;
            decimal costTotal = 0;

            if (cost.Status != CostStageRevisionStatus.Draft && costUser.authLimit.HasValue)
            {
                costTotal = costStageRevision.CostLineItems.Sum(cli => cli.ValueInDefaultCurrency);
                paymentBelowAuthLimit = costUser.authLimit.Value >= costTotal;
            }

            var actionRule = new PgActionRule
            {
                CostStage = costStageRevision.CostStage.Key,
                Status = costStageRevision.Status.ToString(),
                IsRevision = RevisionStages.Contains(costStageRevision.CostStage.Key),
                IsOwner = cost.OwnerId.Equals(userIdentity.Id),
                IsApprover = costUser.isApprover,
                HasPONumber = !string.IsNullOrEmpty(purchaseOrderResponse?.PoNumber),
                NeverSubmitted = isCostNeverSubmitted,
                HasExternalIntegration = cost.IsExternalPurchases,
                CostStageTotal = costTotal,
                CostTotalBelowAuthLimit = paymentBelowAuthLimit,
                IsAdmin = costUser.isAdmin,
                UserIsIPMAndApproved = userIsIPMAndApproved,
                UserIsFinanceManager = costUser.isFinanceManager,
                IsLatestRevision = isLatestRevision
            };

            var actions = await GetActions(actionRule);
            return actions;
        }

        private async Task<Dictionary<string, ActionModel>> GetActions(PgActionRule rule)
        {
            var rules = await _ruleService.GetCompiledByRuleType<PgActionRule>(RuleType.Action);
            var matchFunc = new Func<PgActionRule, dataAccess.Entity.Rule, PgActionRuleDefinition>((t, r) =>
                (PgActionRuleDefinition)JsonConvert.DeserializeObject(r.Definition, typeof(PgActionRuleDefinition))
            );

            var aggregator = new Func<PgActionRuleDefinition, PgActionRuleDefinition, PgActionRuleDefinition>((acc, r) =>
            {
                acc.Actions.AddRange(r.Actions);
                return acc;
            });

            _ruleService.TryMatchRule(rules, rule, matchFunc, aggregator, out var definition);

            return definition?.Actions.ToDictionary(a => a, a => new ActionModel
            {
                Key = (CostAction)Enum.Parse(typeof(CostAction), a)
            }) ?? new Dictionary<string, ActionModel>();
        }
    }
}

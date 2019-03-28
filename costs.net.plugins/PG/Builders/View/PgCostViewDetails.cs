namespace costs.net.plugins.PG.Builders.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders;
    using core.Builders.Search;
    using core.Services;
    using costs.net.core.Builders.Workflow;
    using costs.net.core.Models.ACL;
    using costs.net.core.Models.Approvals;
    using costs.net.core.Models.Costs;
    using costs.net.core.Models.Rule;
    using costs.net.core.Models.User;
    using costs.net.core.Models.Workflow;
    using costs.net.core.Services.Costs;
    using costs.net.core.Services.CostTemplate;
    using costs.net.core.Services.Rules;
    using costs.net.dataAccess.Entity;
    using costs.net.plugins.PG.Extensions;
    using costs.net.plugins.PG.Form;
    using costs.net.plugins.PG.Models;
    using costs.net.plugins.PG.Models.PurchaseOrder;
    using costs.net.plugins.PG.Models.Rules;
    using costs.net.plugins.PG.Services;
    using costs.net.plugins.PG.Services.Costs;
    using costs.net.plugins.PG.Services.PurchaseOrder;
    using dataAccess;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Serilog;

    public class PgCostViewDetails : ICostViewDetails
    {
        private readonly IEnumerable<Lazy<IStageBuilder, PluginMetadata>> _stageBuilders;
        private readonly EFContext _efContext;
        private readonly ILogger _logger = Log.ForContext<PgCostViewDetails>();
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IRuleService _ruleService;
        private readonly IPgPurchaseOrderService _purchaseOrderService;
        private readonly ICostTemplateService _costTemplateService;
        private readonly ICostService _costService;

        public PgCostViewDetails(IMapper mapper,
            EFContext efContext,
            IPermissionService permissionService,
            IEnumerable<Lazy<IStageBuilder, PluginMetadata>> stageBuilders,
            IRuleService ruleService,
            IPgPurchaseOrderService purchaseOrderService,
            ICostTemplateService costTemplateService,
            ICostService costService)
        {
            _purchaseOrderService = purchaseOrderService;
            _costTemplateService = costTemplateService;
            _ruleService = ruleService;
            _mapper = mapper;
            _efContext = efContext;
            _permissionService = permissionService;
            _stageBuilders = stageBuilders;
            _costService = costService;
        }

        /// <summary>
        /// This APIs is combined from 12 other APIs from Middle Tier
        /// 1  - let cost = await core.get(costUrl, auth)
        /// 2  - core.get(`${revisionUrl}/line-item`, auth)
        /// 3  - core.get(`${revisionUrl}/expected-assets`, auth)
        /// 4  - core.get(`${costUrl}/custom-form-data/${latestRevision.stageDetailsId}`, auth)
        /// 5  - core.get(`${costUrl}/custom-form-data/${latestRevision.productDetailsId}`, auth)
        /// 6  - core.get(`${revisionUrl}/supporting-documents`, auth)
        /// 7  - core.get(`custom-data/${latestRevision.id}`, auth)
        /// 8  - core.get(`costs/${data.costId}/stage`, auth)
        /// 9  - core.get(`${costUrl}/workflow/stages`, auth)
        /// 10 - core.get(`${costUrl}/revisions/${data.revisionId}/workflow/actions`, auth)
        /// 11 - core.get(`${costUrl}/watchers`, auth)
        /// 12 - core.get(`${revisionUrl}/approvals`, auth)
        /// 13 - core.get(`costs/${data.costId}/stage/${data.costId}/revision/${data.revisionId}/previous`, auth)
        /// 14 - core.get(`${costUrl}/stage/${data.costId}/revision/${revisionId}`, auth)
        /// 15 - core.get(`costs/${request.costId}/stage/${request.stageId}/revision/${request.revisionId}/travelcosts`, auth)
        /// 16 - core.get(`${costUrl}/stage/latest`, auth)
        /// 17 - core.get(`project/${projectId}?`, auth)
        /// 18 - core.get(`costs/${data.costId}/stage/getstageslatestrevision`, auth)
        /// </summary>
        /// <param name="costId"></param>
        /// <param name="userIdentity"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        public async Task<CostDetailModel> GetCostDetails(Guid costId, UserIdentity userIdentity, Guid revisionId)
        {
            var result = new CostDetailModel();
            try
            {
                var cost = await _efContext.Cost
                            .Include(c => c.PaymentCurrency)
                            .Include(c => c.Project)
                            .Include(c => c.CreatedBy).ThenInclude(creator => creator.Agency).ThenInclude(a => a.Country)
                            .Include(c => c.Owner)
                            .Include(c => c.NotificationSubscribers).ThenInclude(c => c.CostUser).ThenInclude(cu => cu.UserBusinessRoles).ThenInclude(ubr => ubr.BusinessRole)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.TravelCosts).ThenInclude(tv => tv.Region)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.TravelCosts).ThenInclude(tv => tv.Country)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CostStageRevisionPaymentTotals)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CostLineItems)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.StageDetails)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.ProductDetails)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.SupportingDocuments).ThenInclude(sd => sd.SupportingDocumentRevisions)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CustomObjectData)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CostFormDetails).ThenInclude(csr => csr.CustomFormData)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.ApprovalMembers).ThenInclude(am => am.CostUser).ThenInclude(cu => cu.UserBusinessRoles).ThenInclude(ubr => ubr.BusinessRole)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.Requisitioners).ThenInclude(am => am.CostUser).ThenInclude(cu => cu.UserBusinessRoles).ThenInclude(ubr => ubr.BusinessRole)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.ApprovalMembers).ThenInclude(r => r.RejectionDetails)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.Requisitioners).ThenInclude(r => r.RejectionDetails)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.ApprovalMembers).ThenInclude(r => r.ApprovalDetails)
                            .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.Requisitioners).ThenInclude(r => r.ApprovalDetails)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == costId && !c.Deleted);

                // 1
                if (cost == null || !cost.CostStages.Any())
                {
                    throw new Exception("");
                }
                result.Cost = _mapper.Map<CostModel>(cost);

                // 2 + 4 + 5 + 6 + 7
                var selectedRevision = cost.GetSelectedRevision(revisionId);
                if (selectedRevision == null)
                {
                    throw new Exception("");
                }
                result.SelectedRevision = _mapper.Map<CostStageRevisionViewModel>(selectedRevision);
                result.SelectedRevision.CostTemplateVersion = await _costTemplateService.GetCostTemplateVersion(cost.CostTemplateVersionId);
                result.SelectedRevision.TemplateVersion = await _costTemplateService.GetCostTemplate(result.SelectedRevision.CostTemplateVersion.TemplateId);

                // 3 - core.get(`${revisionUrl}/expected-assets`, auth)
                result.SelectedRevision.ExpectedAssets = await GetExpectedAssets(selectedRevision.Id);

                // 8 - core.get(`costs/${data.costId}/stage`, auth)
                var allExchangeRatesInCost = await _costService.GetExchangeRatesOfCurrenciesInCost(cost);
                var defaultPaymentCurrencyId = cost.PaymentCurrencyId.HasValue
                    ? cost.PaymentCurrencyId.Value
                    : await _efContext.Currency.Where(a => a.DefaultCurrency).Select(a => a.Id).FirstAsync();

                result.CostStages.AddRange(cost.CostStages.Select(cs => cs.ToCostStageModel(allExchangeRatesInCost, defaultPaymentCurrencyId)));

                // 9 - core.get(`${costUrl}/workflow/stages`, auth)
                var builder = _stageBuilders.First(b => b.Metadata.BuType == userIdentity.BuType).Value;
                result.StageWorkflow = await builder.GetStages(cost.CostType, selectedRevision);

                // 10 - core.get(`${costUrl}/revisions/${data.revisionId}/workflow/actions`, auth)
                result.ActionsOnCost = await GetCostActions(userIdentity, cost, selectedRevision, cost.CostStages.SelectMany(cs => cs.CostStageRevisions).Count());

                // 11 - watcher
                result.Watchers = _mapper.Map<List<CostWatcherModel>>(cost.NotificationSubscribers);

                // 12 - approvals          
                result.SelectedRevision.Approvals = _mapper.Map<ICollection<Approval>, List<ApprovalModel>>(selectedRevision.Approvals);

                // 13 - Previous revision - used to compare with current revision
                var previousRevision = selectedRevision.GetPreviousRevision(cost);
                if (previousRevision != null)
                {
                    result.PreviousRevision = _mapper.Map<CostStageRevisionViewModel>(previousRevision);
                }

                // 14 - OE revision - revision at OE stage - used to compare with current revisions on view mode
                var oeRevisions = cost.GetLatestRevisionByStage(core.Constants.CostStageConstants.OriginalEstimate);
                if (oeRevisions != null)
                {
                    result.OeRevision = _mapper.Map<CostStageRevisionViewModel>(oeRevisions);
                }

                // 15 - travel costs
                if (selectedRevision.TravelCosts.Any())
                {
                    result.SelectedRevision.TravelCosts = _mapper.Map<ICollection<TravelCost>, List<TravelCostModel>>(selectedRevision.TravelCosts);
                }

                if (oeRevisions?.TravelCosts != null && oeRevisions.TravelCosts.Any())
                {
                    result.OeRevision.TravelCosts = _mapper.Map<ICollection<TravelCost>, List<TravelCostModel>>(oeRevisions.TravelCosts);
                }

                // 16 - latest stage - core.get(`${costUrl}/stage/latest`, auth);
                var latestStage = cost.GetLatestStage();
                if (latestStage != null)
                {
                    result.LatestStage = _mapper.Map<CostStageLatestModel>(latestStage);
                }

                // 17 - Project
                result.Project = _mapper.Map<ProjectViewModel>(cost.Project);

                // 18 - get latest stages
                result.Payments = cost.GetPaymentsViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return result;
        }


        private async Task<List<ExpectedAssetViewModel>> GetExpectedAssets(Guid costStageRevisionId)
        {
            var expectedAssets = await (from ea in _efContext.ExpectedAsset
                                        join mt in _efContext.DictionaryEntry on ea.MediaTypeId equals mt.Id
                                        join project in _efContext.ProjectAdId on ea.ProjectAdIdId equals project.Id
                                        join ov in _efContext.DictionaryEntry on ea.OvalTypeId equals ov.Id into lastData
                                        from last in lastData.DefaultIfEmpty()
                                        where ea.CostStageRevisionId == costStageRevisionId
                                        select new ExpectedAssetViewModel(ea, project)
                                        {
                                            MediaType = mt.Value,
                                            OvalType = last == null ? string.Empty : last.Value
                                        })
                      .ToListAsync();

            return expectedAssets;
        }

        /// <summary>
        /// Get corresponding actions of user on cost
        /// </summary>
        /// <param name="userIdentity">current user</param>
        /// <param name="cost">cost must have its data - dont need any other related table</param>
        /// <param name="costStageRevision">revision must have data of StageDetails / CustomObjectData / Approvals / CostLineItems / Cost Stage tables</param>
        /// <param name="revisionsCount">total number of revisions in the same stage</param>
        private async Task<Dictionary<string, ActionModel>> GetCostActions(UserIdentity userIdentity, Cost cost, CostStageRevision costStageRevision, int revisionsCount)
        {
            var actionRules = await _ruleService.GetCompiledByRuleType<PgActionRule>(RuleType.Action);
            var stageDetails = JsonConvert.DeserializeObject<PgStageDetailsForm>(costStageRevision.StageDetails.Data);
            var smoName = stageDetails.SmoName;

            var costUser = await _efContext.CostUser
                .Where(x => x.Id == userIdentity.Id)
                .Select(cu => new
                {
                    isApprover = cu.UserUserGroups.Any(uug => uug.UserGroup.ObjectId == cost.Id && uug.UserGroup.Role.Name == Roles.CostApprover),
                    isAdmin = cu.UserUserGroups.Any(x => x.UserGroup.Role.Name == Roles.ClientAdmin && x.UserGroup.ObjectId == userIdentity.ModuleId),
                    authLimit = cu.ApprovalLimit,
                    isFinanceManager = cu.UserBusinessRoles.Any(ubr => ubr.BusinessRole != null && ubr.BusinessRole.Key == Constants.BusinessRole.FinanceManager && (ubr.ObjectId != null || ubr.Labels.Contains(smoName)))
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var dataItem = costStageRevision.CustomObjectData.FirstOrDefault(i => i.Name == CustomObjectDataKeys.PgPurchaseOrderResponse);

            var purchaseOrderResponse = dataItem != null
                ? JsonConvert.DeserializeObject<PgPurchaseOrderResponse>(dataItem.Data)
                : new PgPurchaseOrderResponse();

            // current user is IPM user and is approved
            var userIsIPMAndApproved = costStageRevision.Approvals
                .Any(s => s.ApprovalMembers.Any(a => a.MemberId == userIdentity.Id && !a.IsExternal && a.Status == ApprovalStatus.Approved));

            var isLatestRevision = cost.LatestCostStageRevisionId == costStageRevision.Id;

            var paymentBelowAuthLimit = true;
            decimal costTotal = 0;

            // ADC-812, ADC-820
            if (cost.Status != CostStageRevisionStatus.Draft && costUser.authLimit.HasValue)
            {
                costTotal = costStageRevision.CostLineItems.Sum(cli => cli.ValueInDefaultCurrency);
                paymentBelowAuthLimit = costUser.authLimit.Value >= costTotal;
            }

            var actionRule = new PgActionRule
            {
                CostStage = costStageRevision.CostStage.Key,
                Status = costStageRevision.Status.ToString(),
                IsRevision = Constants.RevisionStages.Contains(costStageRevision.CostStage.Key),
                IsOwner = cost.OwnerId.Equals(userIdentity.Id),
                IsApprover = costUser.isApprover,
                HasPONumber = !string.IsNullOrEmpty(purchaseOrderResponse?.PoNumber),
                NeverSubmitted = revisionsCount == 1 && costStageRevision.Status == CostStageRevisionStatus.Draft,
                HasExternalIntegration = cost.IsExternalPurchases,
                CostStageTotal = costTotal,
                CostTotalBelowAuthLimit = paymentBelowAuthLimit,
                IsAdmin = costUser.isAdmin,
                UserIsIPMAndApproved = userIsIPMAndApproved,
                UserIsFinanceManager = costUser.isFinanceManager,
                IsLatestRevision = isLatestRevision
            };

            var actions = GetActions(actionRule, actionRules);
            return actions;
        }

        private Dictionary<string, ActionModel> GetActions(PgActionRule rule, IEnumerable<CompiledRule<PgActionRule>> rules)
        {
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

        public async Task<dynamic> GetXMGOrder(string costNumber)
        {
            return await _purchaseOrderService.GetXMGOrder(costNumber);
        }
    }
}

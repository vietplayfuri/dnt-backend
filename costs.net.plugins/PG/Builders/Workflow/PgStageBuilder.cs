namespace costs.net.plugins.PG.Builders.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Workflow;
    using core.Models.Rule;
    using core.Models.Workflow;
    using core.Services.Costs;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Models.Rules;
    using Models.Stage;
    using MoreLinq;
    using Newtonsoft.Json;
    using Services.Costs;
    using Rule = dataAccess.Entity.Rule;

    public class PgStageBuilder : IStageBuilder, IPgStageBuilder
    {
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly IPgCostService _pgCostService;
        private readonly EFContext _efContext;
        private readonly IRuleService _ruleService;

        public PgStageBuilder(IRuleService ruleService,
            EFContext efContext,
            ICostStageRevisionService costStageRevisionService,
            IPgCostService pgCostService
        )
        {
            _ruleService = ruleService;
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
            _pgCostService = pgCostService;
        }

        public async Task<Dictionary<string, StageModel>> GetStages(PgStageRule stageRule, Guid? vendorId = null)
        {
            var rules = new List<CompiledRule<PgStageRule>>();

            var commonStageRules = await _ruleService.GetCompiledByRuleType<PgStageRule>(RuleType.Stage);
            rules.AddRange(commonStageRules);
            if (vendorId.HasValue)
            {
                rules.AddRange(await _ruleService.GetCompiledByVendorId<PgStageRule>(vendorId.Value, RuleType.VendorStage, null));
            }

            return GetStages(stageRule, rules);
        }

        public async Task<Dictionary<string, StageModel>> GetStages(Guid costId)
        {
            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(r => r.CostStage)
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(r => r.CostLineItems)
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(d => d.StageDetails)
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(r => r.ProductDetails)
                .FirstAsync(c => c.Id == costId);

            var stageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(cost.LatestCostStageRevision);
            var productionDetails = _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(cost.LatestCostStageRevision);
            var revisionTotals = await _pgCostService.GetRevisionTotals(cost.LatestCostStageRevision);

            var stageRule = new PgStageRule
            {
                ProductionType = stageDetails.ProductionType?.Key,
                ContentType = stageDetails.ContentType?.Key,
                BudgetRegion = stageDetails.BudgetRegion?.Key,
                CostType = cost.CostType.ToString(),
                TargetBudgetAmount = stageDetails.InitialBudget.GetValueOrDefault(),
                IsAIPE = stageDetails.IsAIPE,
                TotalCostAmount = revisionTotals.total
            };

            return await GetStages(stageRule, productionDetails?.DirectPaymentVendor?.Id);
        }

        /// <summary>
        /// Build Stage and its rules
        /// <para>Required: Cost revision must have data of Cost Line Item / Stage Details / Production Details</para>
        /// </summary>
        public async Task<Dictionary<string, StageModel>> GetStages(CostType costType, CostStageRevision costStageRevision)
        {
            var stageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevision);
            var productionDetails = _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(costStageRevision);
            var revisionTotals = costStageRevision.CostLineItems.Sum(cli => cli.ValueInDefaultCurrency);

            var stageRule = new PgStageRule
            {
                ProductionType = stageDetails.ProductionType?.Key,
                ContentType = stageDetails.ContentType?.Key,
                BudgetRegion = stageDetails.BudgetRegion?.Key,
                CostType = costType.ToString(),
                TargetBudgetAmount = stageDetails.InitialBudget.GetValueOrDefault(),
                IsAIPE = stageDetails.IsAIPE,
                TotalCostAmount = revisionTotals
            };

            return await GetStages(stageRule, productionDetails?.DirectPaymentVendor?.Id);
        }

        private Dictionary<string, StageModel> GetStages(PgStageRule stageRule, List<CompiledRule<PgStageRule>> rules)
        {
            var matchFunc = new Func<PgStageRule, Rule, PgStageRuleDefinition>((t, r) =>
                (PgStageRuleDefinition) JsonConvert.DeserializeObject(r.Definition, typeof(PgStageRuleDefinition))
            );

            var aggregator = new Func<PgStageRuleDefinition, PgStageRuleDefinition, PgStageRuleDefinition>((acc, r) =>
            {
                // Add stages/transitions according to the rule
                foreach (var stage in r.Add.Stages.Keys)
                {
                    if (!acc.Add.Stages.ContainsKey(stage))
                    {
                        acc.Add.Stages.Add(stage, r.Add.Stages[stage]);
                    }
                    else
                    {
                        // Override stage
                        acc.Add.Stages[stage] = r.Add.Stages[stage];
                    }
                }

                foreach (var stage in r.Add.Transitions.Keys)
                {
                    if (!acc.Add.Transitions.ContainsKey(stage))
                    {
                        acc.Add.Transitions.Add(stage, r.Add.Transitions[stage]);
                    }
                    else
                    {
                        // Add additional transitions
                        acc.Add.Transitions[stage] = acc.Add.Transitions[stage].OrderedMerge(r.Add.Transitions[stage]);
                    }
                }

                // Remove stages/transitions according to the rule
                foreach (var stage in r.Remove.Stages)
                {
                    if (acc.Add.Stages.ContainsKey(stage))
                    {
                        acc.Add.Stages.Remove(stage);
                    }

                    if (acc.Add.Transitions.ContainsKey(stage))
                    {
                        acc.Add.Transitions.Remove(stage);
                    }
                }

                foreach (var key in acc.Add.Transitions.Keys.ToArray())
                {
                    acc.Add.Transitions[key] = acc.Add.Transitions[key].Except(r.Remove.Stages);
                }

                foreach (var key in r.Remove.Transitions.Keys)
                {
                    if (acc.Add.Transitions.ContainsKey(key))
                    {
                        acc.Add.Transitions[key] = acc.Add.Transitions[key].Where(v => !r.Remove.Transitions[key].Contains(v));
                        if (!acc.Add.Transitions[key].Any())
                        {
                            acc.Add.Transitions.Remove(key);
                        }
                    }
                }

                return acc;
            });

            PgStageRuleDefinition definition;
            _ruleService.TryMatchRule(rules, stageRule, matchFunc, aggregator, out definition);

            // Sort stages in reverse topological order
            var stages = Sort(ToStageModes(definition));
            return stages;
        }

        private static Dictionary<string, StageModel> ToStageModes(PgStageRuleDefinition definition)
        {
            if (definition == null)
            {
                return new Dictionary<string, StageModel>();
            }

            var models = new Dictionary<string, StageModel>();
            foreach (var key in definition.Add.Stages.Keys)
            {
                var stage = definition.Add.Stages[key];
                var stageModel = new StageModel
                {
                    Key = key,
                    Name = stage.Name,
                    IsRequired = stage.IsRequired,
                    IsCalculatingPayment = stage.IsCalculatingPayment
                };
                models.Add(key, stageModel);
            }

            foreach (var fromStage in definition.Add.Transitions.Keys)
            {
                foreach (var toStage in definition.Add.Transitions[fromStage])
                {
                    models[fromStage].Transitions.Add(toStage, models[toStage]);
                }
            }

            return models;
        }

        private static Dictionary<string, StageModel> Sort(IReadOnlyDictionary<string, StageModel> stages)
        {
            var initialStage = CostStages.New.ToString();

            if (!stages.ContainsKey(initialStage))
            {
                throw new Exception("No initial stage!");
            }

            var sorted = new Dictionary<string, StageModel>();
            Traverse(stages[initialStage], sorted);

            // Now stages sorted in topological order. Just need to reverse
            sorted = sorted.Reverse().ToDictionary(s => s.Key, s => s.Value);

            return GetLongestPath(sorted, sorted.ToDictionary(s => s.Key, s => new Dictionary<string, int> { { s.Key, 0 } }));
        }

        private static void Traverse(StageModel stage, IDictionary<string, StageModel> acc)
        {
            if (stage.Transitions.Count == 0)
            {
                if (!acc.ContainsKey(stage.Key))
                {
                    acc.Add(stage.Key, stage);
                }

                return;
            }

            foreach (var child in stage.Transitions.Values)
            {
                Traverse(child, acc);
            }

            if (!acc.ContainsKey(stage.Key))
            {
                acc.Add(stage.Key, stage);
            }
        }

        private static Dictionary<string, StageModel> GetLongestPath(
            Dictionary<string, StageModel> stages,
            Dictionary<string, Dictionary<string, int>> paths)
        {
            foreach (var stage in stages.Keys)
            {
                foreach (var transition in stages[stage].Transitions.Keys)
                {
                    if (paths[transition].Count < paths[stage].Count + 1)
                    {
                        var newPath = paths[stage].ToDictionary(s => s.Key, s => s.Value);
                        newPath.Add(transition, paths[stage].Count);
                        paths[transition] = newPath;
                    }
                }
            }

            var endStage = paths.First().Key;
            foreach (var stage in paths.Keys)
            {
                if (paths[stage].Count > paths[endStage].Count)
                {
                    endStage = stage;
                }
            }

            var sorted = paths[endStage];

            foreach (var stage in stages.Keys)
            {
                stages[stage].Transitions = stages[stage].Transitions.OrderBy(t => t.Key, new StageComparer(sorted)).ToDictionary(t => t.Key, t => t.Value);
            }

            stages = stages.OrderBy(s => s.Key, new StageComparer(sorted)).ToDictionary(s => s.Key, s => s.Value);

            return stages;
        }

        private class StageComparer : IComparer<string>
        {
            private readonly Dictionary<string, int> _sortedStages;

            public StageComparer(Dictionary<string, int> sortedStages)
            {
                _sortedStages = sortedStages;
            }

            public int Compare(string x, string y)
            {
                if (!_sortedStages.ContainsKey(x))
                {
                    throw new ArgumentOutOfRangeException(x);
                }

                if (!_sortedStages.ContainsKey(y))
                {
                    throw new ArgumentOutOfRangeException(y);
                }

                return _sortedStages[x].CompareTo(_sortedStages[y]);
            }
        }
    }
}

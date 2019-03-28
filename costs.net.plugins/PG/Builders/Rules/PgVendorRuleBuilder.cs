namespace costs.net.plugins.PG.Builders.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.Rules;
    using core.Extensions;
    using core.Models;
    using core.Models.Rule;
    using core.Services.AbstractTypes;
    using core.Services.Dictionary;
    using core.Services.Regions;
    using costs.net.core.Services.Admin;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Models.Rules;
    using Models.Stage;
    using Models.VendorRule;
    using Newtonsoft.Json;
    using Rule = dataAccess.Entity.Rule;

    public class PgVendorRuleBuilder : IVendorRuleBuilder
    {
        private const string AllOption = "All";

        private static readonly HashSet<string> ValidCostTotalTypes = new HashSet<string>
        {
            Constants.CostSection.CostTotal,
            Constants.CostSection.ProductionInsurance,
            Constants.CostSection.PostProductionInsurance,
            Constants.CostSection.InsuranceTotal,
            Constants.CostSection.PostProduction,
            Constants.CostSection.Production,
            Constants.CostSection.TechnicalFee,
            Constants.CostSection.TargetBudgetTotal,
            Constants.CostSection.Other,
            Constants.CostSection.CostTotal
        };

        private readonly IAbstractTypesService _abstractTypesService;
        private readonly IDictionaryService _dictionaryService;
        private readonly EFContext _efContext;
        private readonly IMapper _mapper;
        private readonly IRegionsService _regionsService;
        private readonly IFeatureService _featureService;

        public PgVendorRuleBuilder(IAbstractTypesService abstractTypesService,
            IDictionaryService dictionaryService,
            IRegionsService regionsService,
            EFContext efContext, IMapper mapper,
            IFeatureService featureService)
        {
            _abstractTypesService = abstractTypesService;
            _dictionaryService = dictionaryService;
            _regionsService = regionsService;
            _efContext = efContext;
            _mapper = mapper;
            _featureService = featureService;
        }

        public async Task<List<CriterionModel>> GetVendorCriteria()
        {
            var module = await _abstractTypesService.GetClientModule(BuType.Pg);

            var dictionaries = await _dictionaryService.GetDictionariesByNames(module.Id, new[]
            {
                Constants.DictionaryNames.CostType,
                Constants.DictionaryNames.ContentType,
                Constants.DictionaryNames.ProductionType
            });
            var result = new List<CriterionModel>
            {
                new CriterionModel
                {
                    FieldName = nameof(PgPaymentRule.BudgetRegion),
                    Values = (await _regionsService.GetAsync()).ToDictionary(i => i.Key, i => i.Name),
                    Operators = new List<string> { ExpressionType.Equal.ToString() },
                    DefaultOperator = ExpressionType.Equal.ToString(),
                    Type = CriterionType.Select
                },
                new CriterionModel
                {
                    FieldName = nameof(PgPaymentRule.CostType),
                    Values = dictionaries.First(a => a.Name == Constants.DictionaryNames.CostType).DictionaryEntries.ToDictionary(i => i.Key, i => i.Value),
                    Operators = new List<string> { ExpressionType.Equal.ToString() },
                    DefaultOperator = ExpressionType.Equal.ToString(),
                    Type = CriterionType.Select
                },
                new CriterionModel
                {
                    FieldName = nameof(PgPaymentRule.ContentType),
                    Values = WithAllOption(dictionaries.First(a => a.Name == Constants.DictionaryNames.ContentType).DictionaryEntries.ToDictionary(i => i.Key, i => i.Value)),
                    Operators = new List<string> { ExpressionType.Equal.ToString() },
                    DefaultOperator = ExpressionType.Equal.ToString(),
                    Type = CriterionType.Select
                },
                new CriterionModel
                {
                    FieldName = nameof(PgPaymentRule.ProductionType),
                    Values = WithAllOption(dictionaries.First(a => a.Name == Constants.DictionaryNames.ProductionType).DictionaryEntries.ToDictionary(i => i.Key, i => i.Value)),
                    Operators = new List<string> { ExpressionType.Equal.ToString() },
                    DefaultOperator = ExpressionType.Equal.ToString(),
                    Type = CriterionType.Select
                }
            };

            var aipeFeature = await _featureService.IsEnabled(core.Constants.Features.Aipe);
            if (aipeFeature != null && aipeFeature.Enabled)
            {
                result.Add(new CriterionModel
                {
                    FieldName = nameof(PgPaymentRule.IsAIPE),
                    Values = new[] { true, false }.ToDictionary(v => v.ToString(), v => v.ToString()),
                    Operators = new List<string> { ExpressionType.Equal.ToString() },
                    DefaultOperator = ExpressionType.Equal.ToString(),
                    Type = CriterionType.Select
                });
            }

            //Move total cost to last item as we don't want to change their orders in UI
            result.Add(new CriterionModel
            {
                FieldName = nameof(PgPaymentRule.TotalCostAmount),
                Operators = new List<string>
                    {
                        ExpressionType.GreaterThanOrEqual.ToString(),
                        ExpressionType.GreaterThan.ToString()
                    },
                DefaultOperator = ExpressionType.GreaterThanOrEqual.ToString(),
                Type = CriterionType.Decimal
            });

            return result;
        }


        public async Task<List<VendorCategoryModel>> GetVendorCategoryModels(List<VendorCategory> vendorCategories)
        {
            if (vendorCategories == null)
            {
                throw new ArgumentException(nameof(vendorCategories));
            }

            var vendorCategoryModels = new List<VendorCategoryModel>();
            foreach (var category in vendorCategories)
            {
                var categoryModel = _mapper.Map<VendorCategoryModel>(category);

                var vendorRulesList = category.VendorCategoryRules ?? new List<VendorRule>(category.VendorCategoryRules);
                if (vendorRulesList.Count == 0)
                {
                    vendorCategoryModels.Add(categoryModel);
                    continue;
                }

                var vendorCriteria = (await GetVendorCriteria()).ToDictionary(vs => vs.FieldName, vs => vs);

                categoryModel.PaymentRules = vendorRulesList.GroupBy(r => r.Name).Select(g =>
                    {
                        var vr = g.First(r => r.Rule.Type == RuleType.VendorPayment);

                        var vendor = new VendorRuleModel
                        {
                            Id = vr.Id,
                            Name = vr.Name,
                            Criteria = BuildVendorCriterial(vr.Rule.Criterion?.Children, vendorCriteria, vr).ToDictionary(c => c.FieldName, c => c),
                            Definition = vr.Rule.Type == RuleType.VendorPayment
                                ? ParseRuleDefinition(vr.Rule.Definition)
                                : null
                        };
                        if (g.Any(r => r.Rule.Type == RuleType.VendorStage))
                        {
                            vendor.SkipFirstPresentation = true;
                        }

                        return vendor;
                    })
                    .ToArray();

                foreach (var fieldName in vendorCriteria.Keys)
                {
                    foreach (var model in categoryModel.PaymentRules)
                    {
                        if (!model.Criteria.ContainsKey(fieldName))
                        {
                            model.Criteria.Add(fieldName,
                                new CriterionValueModel
                                {
                                    FieldName = fieldName,
                                    Value = AllOption,
                                    Operator = ExpressionType.Equal.ToString(),
                                    Text = AllOption
                                });
                        }
                    }
                }
                vendorCategoryModels.Add(categoryModel);
            }

            return vendorCategoryModels;
        }

        public async Task<List<VendorRule>> ValidateAndGetVendorRules(VendorRuleModel ruleModel, Vendor vendor, Guid userId)
        {
            var vendorRules = new List<VendorRule>();

            var definition = JsonConvert.SerializeObject(ruleModel.Definition);
            var paymentRuleDefinition = GetPaymentRuleDefinition(definition);
            vendorRules.Add(await GetVendorRule(ruleModel, paymentRuleDefinition, userId, RuleType.VendorPayment, vendor));

            if (ruleModel.SkipFirstPresentation)
            {
                var stageRuleDefinition = await GetSkipFirstPresentationRule();
                vendorRules.Add(await GetVendorRule(ruleModel, stageRuleDefinition, userId, RuleType.VendorStage, vendor));
            }

            return vendorRules;
        }

        public async Task<List<VendorRule>> ValidateAndGetVendorRules(VendorRuleModel[] categoryPaymentRuleModels, Vendor vendor, Guid userId)
        {
            var vendorRules = new List<VendorRule>();
            foreach (var ruleModel in categoryPaymentRuleModels)
            {
                var definition = JsonConvert.SerializeObject(ruleModel.Definition);
                var paymentRuleDefinition = GetPaymentRuleDefinition(definition);
                vendorRules.Add(await GetVendorRule(ruleModel, paymentRuleDefinition, userId, RuleType.VendorPayment, vendor));

                if (ruleModel.SkipFirstPresentation)
                {
                    var stageRuleDefinition = await GetSkipFirstPresentationRule();
                    vendorRules.Add(await GetVendorRule(ruleModel, stageRuleDefinition, userId, RuleType.VendorStage, vendor));
                }
            }

            return vendorRules;
        }

        private async Task<List<RuleCriterion>> GetRuleCriteria(IReadOnlyDictionary<string, CriterionValueModel> criteriaModel)
        {
            var result = new List<RuleCriterion>();
            var availableCriteria = (await GetVendorCriteria()).ToDictionary(c => c.FieldName, c => c);
            foreach (var criterionValue in criteriaModel)
            {
                if (criterionValue.Value.Value == AllOption)
                {
                    continue;
                }

                if (!IsCriterionValid(criterionValue.Key, criterionValue.Value, availableCriteria, out var errorMessage))
                {
                    throw new Exception(errorMessage);
                }

                result.Add(new RuleCriterion
                {
                    FieldName = criterionValue.Key,
                    Operator = criterionValue.Value.Operator,
                    TargetValue = criterionValue.Value.Value
                });
            }

            return result;
        }

        private Dictionary<string, dynamic> ParseRuleDefinition(string definition)
        {
            var paymentRuleDefinition = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(definition);
            var splits = paymentRuleDefinition.Splits
                .Select(s => new PgVendorRuleSplit
                {
                    CostTotalType = s.CostTotalName,
                    StageSplits = new Dictionary<string, decimal?>
                    {
                        { CostStages.Aipe.ToString(), s.AIPESplit },
                        { CostStages.OriginalEstimate.ToString(), s.OESplit },
                        { CostStages.FirstPresentation.ToString(), s.FPSplit },
                        { CostStages.FinalActual.ToString(), s.FASplit }
                    }
                })
                .ToArray();

            var result = new Dictionary<string, dynamic>
            {
                { nameof(PgVendorRuleDefinition.Splits).ToCamelCase(), splits }
            };
            return result;
        }

        private static decimal GetSplit(IReadOnlyDictionary<string, decimal?> splits, CostStages costStage)
        {
            var percent = splits.ContainsKey(costStage.ToString()) ? splits[costStage.ToString()] : 0;
            if (percent < (decimal)0.0 || percent > (decimal)1.0)
            {
                throw new Exception("Payment split can't be outside 0 - 1 range.");
            }

            return percent ?? 0;
        }

        private static Dictionary<string, string> WithAllOption(IEnumerable<KeyValuePair<string, string>> options)
        {
            var result =
                new Dictionary<string, string> { { AllOption, AllOption } }
                    .Concat(options)
                    .ToDictionary(d => d.Key, d => d.Value);

            return result;
        }

        private static string ValidateCostTotalType(string costTotalType)
        {
            if (string.IsNullOrEmpty(costTotalType) || !ValidCostTotalTypes.Contains(costTotalType))
            {
                throw new Exception($"Unsupported Cost Total Type {costTotalType}");
            }

            return costTotalType;
        }

        private static string GetPaymentRuleDefinition(string definition)
        {
            var splits = JsonConvert.DeserializeObject<PgVendorRuleDefinition>(definition).Splits;
            var pgRuleSplits = splits
                .Select(s =>
                    new PgPaymentRuleDefinitionSplit
                    {
                        CostTotalName = ValidateCostTotalType(s.CostTotalType),
                        AIPESplit = GetSplit(s.StageSplits, CostStages.Aipe),
                        OESplit = GetSplit(s.StageSplits, CostStages.OriginalEstimate),
                        FPSplit = GetSplit(s.StageSplits, CostStages.FirstPresentation),
                        FASplit = GetSplit(s.StageSplits, CostStages.FinalActual)
                    }
                )
                .ToArray();

            var paymentRuleDefinition = new PgPaymentRuleDefinition
            {
                DetailedSplit = !(pgRuleSplits.Length == 1 && pgRuleSplits[0].CostTotalName == Constants.CostSection.CostTotal),
                Splits = pgRuleSplits
            };
            var ruleDefinition = JsonConvert.SerializeObject(paymentRuleDefinition);
            return ruleDefinition;
        }

        private Task<string> GetSkipFirstPresentationRule()
        {
            return _efContext.Rule
                .Where(r => r.Type == RuleType.Stage && r.Name == Constants.Rules.Stage.SkipFirstPresentation)
                .Select(r => r.Definition)
                .FirstOrDefaultAsync();
        }

        private async Task<VendorRule> GetVendorRule(VendorRuleModel ruleModel, string ruleDefinition, Guid userId, RuleType ruleType, Vendor vendor)
        {
            var criterion = ruleModel.Criteria != null
                ? new RuleCriterion
                {
                    Operator = ExpressionType.And.ToString(),
                    Children = await GetRuleCriteria(ruleModel.Criteria)
                }
                : null;

            var ruleEntity = new Rule(userId)
            {
                Name = $"{vendor.Name}_{ruleType}_{ruleModel.Name}",
                Criterion = criterion,
                Definition = ruleDefinition,
                Type = ruleType
            };

            var vendorRule = new VendorRule
            {
                Name = ruleModel.Name,
                Rule = ruleEntity
            };

            return vendorRule;
        }

        private bool IsCriterionValid(string key, CriterionValueModel valueModel, Dictionary<string, CriterionModel> availaleCriteria, out string errorMessage)
        {
            if (string.IsNullOrEmpty(key) || !availaleCriteria.ContainsKey(key))
            {
                errorMessage = $"Unsupported criterion {key}";
                return false;
            }

            var criterion = availaleCriteria[key];

            if (string.IsNullOrEmpty(valueModel.Value) || criterion.Type == CriterionType.Select && !criterion.Values.ContainsKey(valueModel.Value))
            {
                errorMessage = $"Unsupported value {valueModel.Value} of {key} criterion";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public List<CriterionValueModel> BuildVendorCriterial(List<RuleCriterion> criterions, Dictionary<string, CriterionModel> vendorCriteria, VendorRule vr)
        {
            var result = new List<CriterionValueModel>();
            foreach (var c in criterions)
            {
                if (!vendorCriteria.ContainsKey(c.FieldName))
                {
                    continue;
                }

                var criterion = vendorCriteria[c.FieldName];

                if (string.IsNullOrEmpty(c.TargetValue))
                {
                    throw new NullReferenceException($"TargetValue for Criterion in Rule {vr.RuleId} is null.");
                }

                if (criterion.Values != null && !criterion.Values.ContainsKey(c.TargetValue))
                {
                    throw new NullReferenceException($"Criterion value {c.TargetValue} in Rule {vr.RuleId} can not be found.");
                }

                result.Add(new CriterionValueModel
                {
                    FieldName = c.FieldName,
                    Operator = c.Operator,
                    Value = c.TargetValue,
                    Text = criterion.Type == CriterionType.Select
                        ? criterion.Values[c.TargetValue]
                        : c.TargetValue
                });
            }

            return result;
        }
    }
}

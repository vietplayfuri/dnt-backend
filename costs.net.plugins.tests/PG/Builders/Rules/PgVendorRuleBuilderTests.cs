namespace costs.net.plugins.tests.PG.Builders.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Models;
    using core.Models.Regions;
    using core.Models.Rule;
    using core.Services.AbstractTypes;
    using core.Services.Dictionary;
    using core.Services.Regions;
    using costs.net.core.Services.Admin;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Builders.Rules;
    using plugins.PG.Models.Rules;
    using plugins.PG.Models.Stage;
    using plugins.PG.Models.VendorRule;
    using Module = core.Models.AbstractTypes.Module;
    using Rule = dataAccess.Entity.Rule;

    [TestFixture]
    public class PgVendorRuleBuilderTests
    {
        [SetUp]
        public void Init()
        {
            _abstractTypeServiceMock = new Mock<IAbstractTypesService>();
            _dictionaryServiceMock = new Mock<IDictionaryService>();
            _regionServiceMock = new Mock<IRegionsService>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _mapperMock = new Mock<IMapper>();
            _featureService = new Mock<IFeatureService>();

            _pgVendorRuleBuilder = new PgVendorRuleBuilder(
                _abstractTypeServiceMock.Object,
                _dictionaryServiceMock.Object,
                _regionServiceMock.Object,
                _efContext,
                _mapperMock.Object,
                _featureService.Object
            );
        }

        private Mock<IAbstractTypesService> _abstractTypeServiceMock;
        private Mock<IDictionaryService> _dictionaryServiceMock;
        private Mock<IRegionsService> _regionServiceMock;
        private Mock<IMapper> _mapperMock;
        private EFContext _efContext;
        private Mock<IFeatureService> _featureService;
        private PgVendorRuleBuilder _pgVendorRuleBuilder;

        private void MockCriteria()
        {
            var module = new Module { Id = Guid.NewGuid() };
            var dictionnaries = new List<Dictionary>
            {
                new Dictionary
                {
                    Name = nameof(Constants.DictionaryNames.CostType),
                    DictionaryEntries = new List<DictionaryEntry>()
                },
                new Dictionary
                {
                    Name = nameof(Constants.DictionaryNames.ContentType),
                    DictionaryEntries = new List<DictionaryEntry>()
                },
                new Dictionary
                {
                    Name = nameof(Constants.DictionaryNames.ProductionType),
                    DictionaryEntries = new List<DictionaryEntry>()
                }
            };
            _abstractTypeServiceMock.Setup(m => m.GetClientModule(BuType.Pg)).ReturnsAsync(module);
            _dictionaryServiceMock.Setup(m =>
                    m.GetDictionariesByNames(module.Id, It.IsAny<string[]>(), It.IsAny<bool>())
                )
                .ReturnsAsync(dictionnaries);
            _regionServiceMock.Setup(m => m.GetAsync()).ReturnsAsync(new List<RegionModel>());
        }

        public class ValidateAndGetVendorRules : PgVendorRuleBuilderTests
        {
            [Test]
            public async Task Always_Should_MapStageSplitsCorrectly()
            {
                // Arrange
                const decimal aipeSplit = 0.1m;
                const decimal oeSplit = 0.2m;
                const decimal fpSplit = 0.3m;
                const decimal faSplit = 1.0m;

                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.TargetBudgetTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.Aipe.ToString(), aipeSplit },
                                { CostStages.OriginalEstimate.ToString(), oeSplit },
                                { CostStages.FirstPresentation.ToString(), fpSplit },
                                { CostStages.FinalActual.ToString(), faSplit }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act
                var result = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResult = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(result.First().Rule.Definition);

                // Assert
                parsedResult.Splits.Should().HaveCount(1);
                parsedResult.Splits[0].AIPESplit.Should().Be(aipeSplit);
                parsedResult.Splits[0].OESplit.Should().Be(oeSplit);
                parsedResult.Splits[0].FPSplit.Should().Be(fpSplit);
                parsedResult.Splits[0].FASplit.Should().Be(faSplit);
            }

            [Test]
            public async Task DetailedSplit_When_MultipleSplits_ShouldBeTrue()
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.CostTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), (decimal) 1.0 }
                            }
                        },
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.Production,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), (decimal) 1.0 }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act
                var result = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResult = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(result.First().Rule.Definition);

                // Assert
                parsedResult.DetailedSplit.Should().BeTrue();
            }

            [Test]
            public async Task DetailedSplit_When_NotCostsTotal_ShouldBeTrue()
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.Production,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), (decimal) 1.0 }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act
                var result = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResult = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(result.First().Rule.Definition);

                // Assert
                parsedResult.DetailedSplit.Should().BeTrue();
            }

            [Test]
            public async Task DetailedSplit_When_SingleSplit_And_CostsTotal_ShouldBeFalse()
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.CostTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), (decimal) 1.0 }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act
                var result = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResult = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(result.First().Rule.Definition);

                // Assert
                parsedResult.DetailedSplit.Should().BeFalse();
            }

            [Test]
            public async Task Get_GetVendorCategoryModels_valid()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var moduleId = Guid.NewGuid();
                var vendorCategoryName = "Kitty Cat";
                var usd = new Currency
                {
                    Code = "USD",
                    DefaultCurrency = true,
                    Description = "US DOller",
                    Labels = new string[0],
                    Symbol = "$"
                };
                _efContext.Currency.Add(usd);

                var vendorCategories = new List<VendorCategory>
                {
                    new VendorCategory(userId)
                    {
                        Name = vendorCategoryName,
                        VendorCategoryRules = new List<VendorRule>
                        {
                            new VendorRule
                            {
                                Name = "VendorRule",
                                Rule = new Rule(userId)
                                {
                                    Type = RuleType.VendorPayment,
                                    Criteria =
                                        "{\"FieldName\":null,\"Operator\":\"And\",\"TargetValue\":null,\"Children\":[{\"FieldName\":\"BudgetRegion\",\"Operator\":\"Equal\",\"TargetValue\":\"AAK (Asia)\",\"Children\":[]},{\"FieldName\":\"CostType\",\"Operator\":\"Equal\",\"TargetValue\":\"Buyout\",\"Children\":[]},{\"FieldName\":\"IsAIPE\",\"Operator\":\"Equal\",\"TargetValue\":\"False\",\"Children\":[]},{\"FieldName\":\"TotalCostAmount\",\"Operator\":\"GreaterThanOrEqual\",\"TargetValue\":\"12313\",\"Children\":[]}]}",
                                    Criterion = new RuleCriterion
                                    {
                                        Children = new List<RuleCriterion>
                                        {
                                            new RuleCriterion
                                            {
                                                Operator = "Equal",
                                                FieldName = "BudgetRegion",
                                                TargetValue = "AAK (Asia)"
                                            },
                                            new RuleCriterion
                                            {
                                                Operator = "Equal",
                                                FieldName = "CostType",
                                                TargetValue = "Buyout"
                                            },
                                            new RuleCriterion
                                            {
                                                Operator = "Equal",
                                                FieldName = "IsAIPE",
                                                TargetValue = "False"
                                            },
                                            new RuleCriterion
                                            {
                                                Operator = "GreaterThanOrEqual",
                                                FieldName = "TotalCostAmount",
                                                TargetValue = "12313"
                                            }
                                        },
                                        Operator = "And"
                                    },
                                    Definition =
                                        "{\"DetailedSplit\":false,\"Splits\":[{\"CostTotalName\":\"CostTotal\",\"AIPESplit\":0.0,\"OESplit\":0.0,\"FPSplit\":0.0,\"FASplit\":1.0}]}"
                                }
                            }
                        },
                        IsPreferredSupplier = true,
                        HasDirectPayment = false,
                        Currency = usd,
                        DefaultCurrencyId = usd.Id
                    }
                };

                var vendor = new Vendor(userId)
                {
                    Name = "Vendor Name",
                    Labels = new string[0],
                    SapVendor = "123456",
                    Categories = vendorCategories
                };
                _regionServiceMock.Setup(a => a.GetAsync()).ReturnsAsync(new List<RegionModel>
                {
                    new RegionModel { Key = "AAK (Asia)", Name = "AAK" },
                    new RegionModel { Key = "GREATER CHINA AREA", Name = "Greater China" },
                    new RegionModel { Key = "NORTHERN AMERICA AREA", Name = "North America" },
                    new RegionModel { Key = "JAPAN", Name = "Japan" },
                    new RegionModel { Key = "EUROPE AREA", Name = "Europe" },
                    new RegionModel { Key = "INDIA & MIDDLE EAST AFRICA AREA", Name = "IMEA" },
                    new RegionModel { Key = "LATIN AMERICA AREA", Name = "Latin America" }
                });
                _abstractTypeServiceMock.Setup(a => a.GetClientModule(BuType.Pg)).ReturnsAsync(new Module
                {
                    Name = "P&G",
                    Id = moduleId,
                    BuType = BuType.Pg,
                    Key = "P&G"
                });
                _dictionaryServiceMock.Setup(a => a.GetDictionariesByNames(moduleId, new[]
                {
                    Constants.DictionaryNames.CostType,
                    Constants.DictionaryNames.ContentType,
                    Constants.DictionaryNames.ProductionType
                }, true)).ReturnsAsync(new List<Dictionary>
                {
                    new Dictionary
                    {
                        Name = Constants.DictionaryNames.ProductionType,
                        IsNameEditable = false,
                        IsStatic = true,
                        DictionaryEntries = new List<DictionaryEntry>
                        {
                            new DictionaryEntry
                            {
                                Key = Constants.ProductionType.CgiAnimation,
                                Value = Constants.ProductionType.CgiAnimation,
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = Constants.ProductionType.PostProductionOnly,
                                Value = Constants.ProductionType.PostProductionOnly,
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = Constants.ProductionType.FullProduction,
                                Value = Constants.ProductionType.FullProduction,
                                Visible = true
                            }
                        }
                    },
                    new Dictionary
                    {
                        Name = Constants.DictionaryNames.CostType,
                        IsNameEditable = false,
                        IsStatic = true,
                        DictionaryEntries = new List<DictionaryEntry>
                        {
                            new DictionaryEntry
                            {
                                Key = Constants.UsageBuyoutType.Buyout,
                                Value = "Usage/Buyout/Contract",
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = "Production",
                                Value = "Production",
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = "Trafficking",
                                Value = "Trafficking/Distribution",
                                Visible = true
                            }
                        }
                    },
                    new Dictionary
                    {
                        Name = Constants.DictionaryNames.ContentType,
                        IsNameEditable = false,
                        IsStatic = true,
                        DictionaryEntries = new List<DictionaryEntry>
                        {
                            new DictionaryEntry
                            {
                                Key = Constants.ContentType.Audio,
                                Value = Constants.ContentType.Audio,
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = Constants.ContentType.Digital,
                                Value = Constants.ContentType.Digital,
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = Constants.ContentType.Photography,
                                Value = "Still Image",
                                Visible = true
                            },
                            new DictionaryEntry
                            {
                                Key = Constants.ContentType.Video,
                                Value = Constants.ContentType.Video,
                                Visible = true
                            }
                        }
                    }
                });
                _efContext.Vendor.Add(vendor);
                _efContext.SaveChanges();

                _mapperMock.Setup(a => a.Map<VendorCategoryModel>(vendorCategories.First())).Returns(new VendorCategoryModel
                {
                    Name = vendorCategoryName,
                    Id = vendorCategories.First().Id,
                    HasDirectPayment = false,
                    IsPreferredSupplier = true
                });

                //Before implement ADC-2597, default value is always true
                _featureService.Setup(f => f.IsEnabled(It.IsAny<string>())).ReturnsAsync(new core.Models.Admin.Feature
                {
                    Enabled = true
                });

                // Act
                var result = await _pgVendorRuleBuilder.GetVendorCategoryModels(vendorCategories);

                // Assert
                result.Count.Should().Be(1);
                result.First().HasDirectPayment.Should().BeFalse();
                result.First().IsPreferredSupplier.Should().BeTrue();
                result.First().Name.Should().Be(vendorCategoryName);
                result.First().PaymentRules.Length.Should().Be(1);
            }

            [Test]
            public async Task SkipFirstStagePresentation_When_SkipFirstPresentation_ShouldAddSkipFirstPresentationStageRule()
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new PgVendorRuleSplit[0]
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr),
                    SkipFirstPresentation = true
                };
                var vendor = new Vendor { Name = "Vendor 1" };
                var skipFirstPresentationRule = new Rule
                {
                    Name = Constants.Rules.Stage.SkipFirstPresentation,
                    Type = RuleType.Stage,
                    Definition = JsonConvert.SerializeObject(new PgStageRuleDefinition())
                };
                _efContext.Rule.Add(skipFirstPresentationRule);
                _efContext.SaveChanges();

                // Act
                var vendorRules = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResults = vendorRules.Select(r =>
                        JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(r.Rule.Definition))
                    .ToArray();

                //Assert
                parsedResults.Should().HaveCount(2);
                vendorRules.Count(r => r.Rule.Type == RuleType.VendorStage).Should().Be(1);
                vendorRules.Count(r => r.Rule.Type == RuleType.VendorPayment).Should().Be(1);
                var stageRule = vendorRules.First(vr => vr.Rule.Type == RuleType.VendorStage);
                stageRule.Rule.Definition.Should().Be(skipFirstPresentationRule.Definition);
            }

            [Test]
            public void ThrowException_When_CriterionIsSelectAndValueIsInvalid()
            {
                // Arrange
                const string validCriteriaName = nameof(Constants.DictionaryNames.ContentType);
                const string wrongValue = "not existing value";
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new PgVendorRuleSplit[0]
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr),
                    Criteria = new Dictionary<string, CriterionValueModel>
                    {
                        { validCriteriaName, new CriterionValueModel { Value = wrongValue } }
                    }
                };
                var vendor = new Vendor { Name = "Vendor 1" };
                MockCriteria();

                // Act
                // Assert
                _pgVendorRuleBuilder
                    .Awaiting(b => b.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>()))
                    .ShouldThrow<Exception>()
                    .WithMessage($"Unsupported value {wrongValue} of {validCriteriaName} criterion");
            }

            [Test]
            public void ThrowException_When_CriterionNameIsInvalid()
            {
                // Arrange
                const string criteriaName = "unsupported criterion";
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new PgVendorRuleSplit[0]
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr),
                    Criteria = new Dictionary<string, CriterionValueModel>
                    {
                        { criteriaName, new CriterionValueModel() }
                    }
                };
                var vendor = new Vendor { Name = "Vendor 1" };
                MockCriteria();

                // Act
                // Assert
                _pgVendorRuleBuilder
                    .Awaiting(b => b.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>()))
                    .ShouldThrow<Exception>()
                    .WithMessage($"Unsupported criterion {criteriaName}");
            }

            [Test]
            [TestCase(-0.00001)]
            [TestCase(1.00001)]
            public void When_StageSplitIs_LessThanZero_Or_MoreThanOne_ShouldThrowException(decimal split)
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.CostTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), split }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act, Assert
                _pgVendorRuleBuilder
                    .Awaiting(b => b.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>()))
                    .ShouldThrow<Exception>()
                    .WithMessage("Payment split can\'t be outside 0 - 1 range.");
            }

            [Test]
            [TestCase(0)]
            [TestCase(1)]
            public void When_StageSplitIs_Zero_Or_One_ShouldNotThrowException(decimal split)
            {
                // Arrange
                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.CostTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), split }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act, Assert
                _pgVendorRuleBuilder
                    .Awaiting(b => b.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>()))
                    .ShouldNotThrow<Exception>();
            }

            [Test]
            public async Task When_StageSplitIsNotListedInSplits_Should_DefaultSplitToZero()
            {
                // Arrange
                const decimal faSplit = 1.0m;

                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType = Constants.CostSection.TargetBudgetTotal,
                            StageSplits = new Dictionary<string, decimal?>
                            {
                                { CostStages.FinalActual.ToString(), faSplit }
                            }
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act
                var result = await _pgVendorRuleBuilder.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>());
                var parsedResult = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(result.First().Rule.Definition);

                // Assert
                parsedResult.Splits.Should().HaveCount(1);
                parsedResult.Splits[0].AIPESplit.Should().Be(0m);
                parsedResult.Splits[0].OESplit.Should().Be(0m);
                parsedResult.Splits[0].FPSplit.Should().Be(0m);
                parsedResult.Splits[0].FASplit.Should().Be(faSplit);
            }

            [Test]
            public void ThrowException_When_CriterionIsSelectAndValueIsInvalid2()
            {
                // Arrange
                var costTotalType = "Anything else";

                var rule = new PgVendorRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgVendorRuleSplit
                        {
                            CostTotalType =costTotalType,
                            StageSplits = new Dictionary<string, decimal?>()
                        }
                    }
                };
                var ruleStr = JsonConvert.SerializeObject(rule);
                var ruleMode = new VendorRuleModel
                {
                    Name = "Vendor payment rule 1",
                    Definition = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ruleStr)
                };
                var vendor = new Vendor { Name = "Vendor 1" };

                // Act, Assert
                _pgVendorRuleBuilder
                    .Awaiting(b => b.ValidateAndGetVendorRules(ruleMode, vendor, It.IsAny<Guid>()))
                    .ShouldThrow<Exception>()
                    .WithMessage($"Unsupported Cost Total Type {costTotalType}");
            }


            private List<RuleCriterion> CreateListOfCriterions()
            {
                return new List<RuleCriterion> {
                    new RuleCriterion{
                        FieldName = "BudgetRegion",
                        Operator = "Equal",
                        TargetValue = "GREATER CHINA AREA"
                    },
                    new RuleCriterion{
                        FieldName = "CostType",
                        Operator= "Equal",
                        TargetValue = "Production"
                    }
                };
            }
            
            private Dictionary<string, CriterionModel> CreateListOfVendorCriteria()
            {
                return new Dictionary<string, CriterionModel>() {
                    { "BudgetRegion", new CriterionModel() {
                        DefaultOperator = "Equal",
                        FieldName = "BudgetRegion",
                        Operators = new List<string>{ "Equal" },
                        Type = CriterionType.Select,
                        Values = new Dictionary<string, string>{
                                { "AAK (Asia)", "AAK" },
                                { "EUROPE AREA", "Europe" },
                                { "GREATER CHINA AREA", "Greater China" },
                                { "INDIA & MIDDLE EAST AFRICA AREA", "IMEA" },
                                { "JAPAN", "Japan" },
                                { "LATIN AMERICA AREA", "Latin America" },
                                { "NORTHERN AMERICA AREA", "North America" }
                            }
                        }
                    },
                    { "CostType", new CriterionModel() {
                        DefaultOperator = "Equal",
                        FieldName = "CostType",
                        Operators = new List<string>{ "Equal" },
                        Type = CriterionType.Select,
                        Values = new Dictionary<string, string>{
                                { "Production", "Production" },
                                { "Trafficking", "Trafficking/Distributio" },
                                { "Buyout", "Usage/Buyout/Contract" }
                            }
                        }
                    }
                };
            }

            [Test]
            public void BuildVendorCriterial_NumberOf_VendorRule_And_Vendor_Criterions_Are_The_Same()
            {
                // Arrange
                var expectedData = new List<CriterionValueModel> {
                    new CriterionValueModel {
                        FieldName = "BudgetRegion",
                        Operator = "Equal",
                        Text = "Greater China",
                        Value = "GREATER CHINA AREA"
                    },
                    new CriterionValueModel() {
                        FieldName = "CostType",
                        Operator = "Equal",
                        Text = "Production",
                        Value = "Production"
                    }
                };

                var ruleCriterions = CreateListOfCriterions();

                var vendorCriteria = CreateListOfVendorCriteria();

                VendorRule vr = new VendorRule()
                {
                    RuleId = Guid.NewGuid()
                };

                // Act
                var data = _pgVendorRuleBuilder.BuildVendorCriterial(ruleCriterions, vendorCriteria, vr);

                // Assert
                Assert.AreEqual(data.Count, expectedData.Count);
            }

            [Test]
            public void BuildVendorCriterial_NumberOf_Rule_Criterions_Are_Greater_Than_Vendor_Criterions_Return_Common_Criterions()
            {
                // Arrange
                var expectedData = new List<CriterionValueModel> {
                    new CriterionValueModel() {
                        FieldName = "CostType",
                        Operator = "Equal",
                        Text = "Production",
                        Value = "Production"
                    }
                };

                var ruleCriterions = CreateListOfCriterions();

                var vendorCriteria = CreateListOfVendorCriteria();
                vendorCriteria.Remove(vendorCriteria.First().Key);

                VendorRule vr = new VendorRule()
                {
                    RuleId = Guid.NewGuid()
                };

                // Act
                var data = _pgVendorRuleBuilder.BuildVendorCriterial(ruleCriterions, vendorCriteria, vr);

                // Assert
                Assert.AreEqual(data.Count, expectedData.Count);
            }

            [Test]
            public void BuildVendorCriterial_NumberOf_Rule_Criterions_Are_Less_Than_Vendor_Criterions_Return_Common_Criterions()
            {
                // Arrange
                var expectedData = new List<CriterionValueModel> {
                    new CriterionValueModel() {
                        FieldName = "CostType",
                        Operator = "Equal",
                        Text = "Production",
                        Value = "Production"
                    }
                };

                var ruleCriterions = CreateListOfCriterions();
                ruleCriterions.Remove(ruleCriterions.First());

                var vendorCriteria = CreateListOfVendorCriteria();

                VendorRule vr = new VendorRule()
                {
                    RuleId = Guid.NewGuid()
                };

                // Act
                var data = _pgVendorRuleBuilder.BuildVendorCriterial(ruleCriterions, vendorCriteria, vr);

                // Assert
                Assert.AreEqual(data.Count, expectedData.Count);
            }
        }

        [Test]
        public async Task Test_Get_Vendor_Criteria_Aipe_Enable_Return_6_Items_With_Correct_Order()
        {
            // Arrange
            MockCriteria();
            _featureService.Setup(f => f.IsEnabled(It.IsAny<string>())).ReturnsAsync(new core.Models.Admin.Feature
            {
                Enabled = true
            });

            // Act
            var result = await _pgVendorRuleBuilder.GetVendorCriteria();

            // Assert
            result.Should().HaveCount(6);
            result[0].FieldName = nameof(PgPaymentRule.BudgetRegion);
            result[1].FieldName = nameof(PgPaymentRule.CostType);
            result[2].FieldName = nameof(PgPaymentRule.ContentType);
            result[3].FieldName = nameof(PgPaymentRule.ProductionType);
            result[4].FieldName = nameof(PgPaymentRule.IsAIPE);
            result[5].FieldName = nameof(PgPaymentRule.TotalCostAmount);
        }

        [Test]
        public async Task Test_Get_Vendor_Criteria_Aipe_Enable_Return_5_Items_With_Correct_Order()
        {
            // Arrange
            MockCriteria();
            _featureService.Setup(f => f.IsEnabled(It.IsAny<string>())).ReturnsAsync(new core.Models.Admin.Feature
            {
                Enabled = false
            });

            // Act
            var result = await _pgVendorRuleBuilder.GetVendorCriteria();

            // Assert
            result.Should().HaveCount(5);
            result[0].FieldName = nameof(PgPaymentRule.BudgetRegion);
            result[1].FieldName = nameof(PgPaymentRule.CostType);
            result[2].FieldName = nameof(PgPaymentRule.ContentType);
            result[3].FieldName = nameof(PgPaymentRule.ProductionType);
            result[4].FieldName = nameof(PgPaymentRule.TotalCostAmount);
        }

    }
}

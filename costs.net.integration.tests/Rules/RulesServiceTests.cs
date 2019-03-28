namespace costs.net.integration.tests.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Builders.Rules;
    using core.Services.Rules;
    using dataAccess.Entity;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using core.Models;
    using core.Models.Rule;
    using dataAccess;
    using Rule = dataAccess.Entity.Rule;

    [TestFixture]
    public abstract class RulesServiceTests : BaseIntegrationTest
    {
        [SetUp]
        public void Setup()
        {
            Service = new RuleService(
                GetService<IEnumerable<Lazy<IVendorRuleBuilder, PluginMetadata>>>(),
                GetService<IEnumerable<Lazy<IPluginRuleService, PluginMetadata>>>(),
                GetService<IRuleEngine>(),
                GetService<EFContext>()
                );
        }

        protected RuleService Service;
        protected BuType BuType = BuType.Pg;
    }

    [TestFixture]
    public class AudioRulesShould : RulesServiceTests
    {
        [Test]
        public async Task ReturnFalseIfBudgetTooSmall()
        {
            var result = await Service.CheckAipeApplicability("EUROPE AREA", 49000, "Audio", "Full Production", BuType);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task ReturnTrueIfConditionsAreMet()
        {
            var result = await Service.CheckAipeApplicability("EUROPE AREA", 51000, "Audio", "Full Production", BuType);
            Assert.IsTrue(result);
        }
    }

    [TestFixture]
    public class VideoRulesShould : RulesServiceTests
    {
        [Test]
        public async Task ReturnFalseIfPostProductionOnlyAndBudgetTooSmall()
        {
            var result = await Service.CheckAipeApplicability("NORTHERN AMERICA AREA", 2, "Video", "Post Production Only", BuType);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task ReturnTrueIfConditionsAreMet()
        {
            var result = await Service.CheckAipeApplicability("NORTHERN AMERICA AREA", 1, "Video", "Full Production", BuType);
            Assert.IsTrue(result);
        }
    }

    [TestFixture]
    public class TryMatchShould : RulesServiceTests
    {
        private readonly Func<TestRule, Rule, decimal> _matchFunc = (t, r) =>
        {
            var pgPaymentRuleData = JsonConvert.DeserializeObject<TestDefinition>(r.Definition);
            return pgPaymentRuleData.Split * t.TotalCostAmount / 100;
        };

        public class TestRule
        {
            public string BudgetRegion { get; set; }

            public Guid? DirectPaymentVendorId { get; set; }

            public decimal TotalCostAmount { get; set; }
        }

        public class TestDefinition
        {
            public decimal Split { get; set; }
        }

        protected void SetupCommonPaymentRule()
        {
            //const string region = "China";
            //RuleRepository.Setup(r => r.GetByRuleType(RuleType.CommonPayment))
            //    .ReturnsAsync(new List<Rule> { GetBudgetRegionCriterion(region, (decimal) 1.0) });
        }

        protected CompiledRule<TestRule> GetBudgetRegionCriterion(string regionName, decimal split)
        {
            const string regionFieldName = "BudgetRegion";
            var rule = new Rule
            {
                Type = RuleType.CommonPayment,
                Criterion = new RuleCriterion
                {
                    FieldName = regionFieldName,
                    Operator = "Equal",
                    TargetValue = regionName
                },
                Definition = JsonConvert.SerializeObject(new Dictionary<string, dynamic>
                {
                    { "split", split }
                })
            };

            return Service.GetCompiledRule<TestRule>(rule);
        }

        [Test]
        public void GetGoodsReceiptMessage_whenDirectPaymentVendor_shouldUseDirectPaymentRules()
        {
            // Arrange
            SetupCommonPaymentRule();
            var rules = new List<CompiledRule<TestRule>>();

            var directPaymentVendorId = Guid.NewGuid();
            var directVendorSplit = (decimal) 50.00;
            var commonRuleSplit = (decimal) 100.00;
            var totalAmount = 1000;
            var expected = directVendorSplit * totalAmount / 100;
            const string region = "China";

            var testRule = new TestRule
            {
                BudgetRegion = region,
                DirectPaymentVendorId = directPaymentVendorId,
                TotalCostAmount = totalAmount
            };
            rules.Add(GetBudgetRegionCriterion(region, directVendorSplit));

            const string commonPaymentRuleRegion = "Any other region";
            rules.Add(GetBudgetRegionCriterion(commonPaymentRuleRegion, commonRuleSplit));

            // Act
            decimal amount;
            Service.TryMatchRule(rules, testRule, _matchFunc, out amount);

            // Assert
            amount.Should().Be(expected);
        }

        [Test]
        public void GetGoodsReceiptMessage_whenDirectPaymentVendorButNonOfVendorRulesMatch_shouldUseCommonPaymentRules()
        {
            // Arrange
            SetupCommonPaymentRule();
            var dbRules = new List<CompiledRule<TestRule>>();

            var directPaymentVendorId = Guid.NewGuid();
            var directVendorSplit = (decimal) 50.00;
            var commonRuleSplit = (decimal) 100.00;
            var totalAmount = 1000;
            var expected = commonRuleSplit * totalAmount / 100;
            const string region = "China";

            var testRule = new TestRule
            {
                BudgetRegion = region,
                DirectPaymentVendorId = directPaymentVendorId,
                TotalCostAmount = totalAmount
            };

            const string commonPaymentRuleRegion = "Any other region";
            dbRules.Add(GetBudgetRegionCriterion(commonPaymentRuleRegion, directVendorSplit));

            dbRules.Add(GetBudgetRegionCriterion(region, commonRuleSplit));

            // Act
            decimal amount;
            Service.TryMatchRule(dbRules, testRule, _matchFunc, out amount);

            // Assert
            amount.Should().Be(expected);
        }
    }
}
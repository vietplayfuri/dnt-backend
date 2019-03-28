namespace costs.net.integration.tests.Plugins.PG.PaymentRules.Europe
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    public class PaymentRule_Audio : PaymentRuleIntegrationTestBase
    {
        public class TestCase
        {
            public string BudgetRegion => Constants.BudgetRegion.Europe;
            public string ContectType => Constants.ContentType.Audio;

            public string ProductionType { get; set; }
            public bool IsAipe { get; set; }
            public CostStages Stage { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal ExpectedAmount { get; set; }
        }

        //ADC-976
        private static TestCase[] _aipeStageTestCases = {
            new TestCase
            {
                IsAipe = true,
                Stage = CostStages.Aipe,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 50002,
                ExpectedAmount = 25001
            }
        };
        //ADC-976, ADC-981
        private static TestCase[] _otherStagesTestCases = {
            //AIPE, > 50000
            new TestCase
            {
                IsAipe = true,
                Stage = CostStages.OriginalEstimate,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 50002,
                ExpectedAmount = 0
            },
            new TestCase
            {
                IsAipe = true,
                Stage = CostStages.FinalActual,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 50002,
                ExpectedAmount = 50002
            },
            //AIPE, < 50000 is not eligible for Audio (ADC-576)
            //Non-AIPE, > 50000
            new TestCase
            {
                IsAipe = false,
                Stage = CostStages.OriginalEstimate,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 50002,
                ExpectedAmount = 25001
            },
            new TestCase
            {
                IsAipe = false,
                Stage = CostStages.FinalActual,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 50002,
                ExpectedAmount = 50002
            },
            //Non-AIPE, < 50000
            new TestCase
            {
                IsAipe = false,
                Stage = CostStages.OriginalEstimate,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 49998,
                ExpectedAmount = 0
            },
            new TestCase
            {
                IsAipe = false,
                Stage = CostStages.FinalActual,
                ProductionType = Constants.ProductionType.FullProduction,
                TotalAmount = 49998,
                ExpectedAmount = 49998
            }
        };

        [Test]
        [TestCaseSource(nameof(_aipeStageTestCases))]
        public async Task PaymentRule_AIPE_Europe_Audio(TestCase testCase)
        {
            // Arrange
            InitData(
                isAipe: testCase.IsAipe,
                stageKey: testCase.Stage.ToString(),
                budgetRegion: testCase.BudgetRegion,
                targetBudget: testCase.TotalAmount.ToString(CultureInfo.InvariantCulture),
                contentType: testCase.ContectType,
                productionType: testCase.ProductionType
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(testCase.ExpectedAmount);
        }

        [Test]
        [TestCaseSource(nameof(_otherStagesTestCases))]
        public async Task PaymentRule_Europe_Audio(TestCase testCase)
        {
            // Arrange
            InitData(
                isAipe: testCase.IsAipe,
                stageKey: testCase.Stage.ToString(),
                budgetRegion: testCase.BudgetRegion,
                items: new List<CostLineItemView>
                {
                    new CostLineItemView { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = testCase.TotalAmount }
                },
                contentType: testCase.ContectType,
                productionType: testCase.ProductionType
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(testCase.ExpectedAmount);
        }
    }
}

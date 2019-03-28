namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    // Refinement - New Payment Rules ADC-1306
    [TestFixture]
    public class PaymentRule_FullProduction_Audio_nonAIPE : PaymentRuleIntegrationTestBase
    {
        public class TestCaseData
        {
            public string BudgetRegion { get; set; }

            public string Stage { get; set; }

            public decimal Percent { get; set; }
        }

        private static readonly List<TestCaseData> TestCasesMoreThan50000 = new List<TestCaseData>
        {
            // 230
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.NorthAmerica,
                Stage = CostStages.OriginalEstimate.ToString(),
                Percent = 0.50m
            },
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.NorthAmerica,
                Stage = CostStages.FinalActual.ToString(),
                Percent = 1.00m
            },
            // 232
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                Stage = CostStages.OriginalEstimate.ToString(),
                Percent = 0.50m
            },
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                Stage = CostStages.FinalActual.ToString(),
                Percent = 1.00m
            }
        };

        private static readonly List<TestCaseData> TestCasesLethThan50000 = new List<TestCaseData>
        {
            // 231
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.NorthAmerica,
                Stage = CostStages.OriginalEstimate.ToString(),
                Percent = 0.00m
            },
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.NorthAmerica,
                Stage = CostStages.FinalActual.ToString(),
                Percent = 1.00m
            },
            // 233
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                Stage = CostStages.OriginalEstimate.ToString(),
                Percent = 0.00m
            },
            new TestCaseData
            {
                BudgetRegion  = Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                Stage = CostStages.FinalActual.ToString(),
                Percent = 1.00m
            }
        };

        // ADC-1306
        [Test]
        [TestCaseSource(nameof(TestCasesMoreThan50000))]
        public async Task PaymentRule_FullProduction_Audio_GreaterThan_50000(TestCaseData caseDate)
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: caseDate.Stage,
                budgetRegion: caseDate.BudgetRegion,
                items: new List<CostLineItemView>
                {
                    new CostLineItemView { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 50000 },
                    new CostLineItemView { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(50100 * caseDate.Percent);
        }

        // ADC-1306
        [Test]
        [TestCaseSource(nameof(TestCasesLethThan50000))]
        public async Task PaymentRule_FullProduction_Audio_LessThan_50000(TestCaseData caseDate)
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: caseDate.Stage,
                budgetRegion: caseDate.BudgetRegion,
                items: new List<CostLineItemView>
                {
                    new CostLineItemView { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 100 },
                    new CostLineItemView { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(200 * caseDate.Percent);
        }

    }
}

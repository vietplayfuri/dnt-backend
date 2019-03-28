namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    public class PaymentRule_FP_nonAIPE : PaymentRuleIntegrationTestBase
    {
        // rule name - "FullProduction-NonAIPE-NonDPV"
        [Test]
        public async Task PaymentRule_NonAIPE_1_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "300",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 100 }
                }
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // we should have 100% of the production insurance
            receipt.PaymentAmount.ShouldBeEquivalentTo(100);
        }

        [Test]
        public async Task PaymentRule_NonAIPE_1_A_FP_stage()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "SOMETHINGELSE", ValueInDefaultCurrency = 999 }}
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // we should have 50% of post production and 100% of production costs
            receipt.PaymentAmount.ShouldBeEquivalentTo(150);
        }

        [Test]
        public async Task PaymentRule_NonAIPE_1_FP_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 250 },//(incl. insurance)
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = 100 }
                }
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // production = 150 - 100 = 50 * 100%
            // post production = 100 * 50%
            // insurance = 100 * 100%
            // technical fee = 100 * 100%
            // all other costs = 100 * 0%
            // sum = 50 + 50 + 100 + 100 + 0 = 300
            receipt.PaymentAmount.ShouldBeEquivalentTo(300);
        }

        [Test]
        public async Task PaymentRule_NonAIPE_1_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 150 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.TechnicalFee, LineItemTotalCalculatedValue = 1000 }
                }
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // production = 150 * 100%
            // post production = 100 * 100%
            // insurance = 100 * 100%
            // technical fee = 100 - 1000 = -900 * 100%
            // all other costs = 100 * 100%
            // sum = -850 + 100 + 100 + 100 + 100 = -450
            receipt.PaymentAmount.ShouldBeEquivalentTo(-450);
        }
    }
}

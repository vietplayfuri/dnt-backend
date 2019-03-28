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

    public class PaymentRule_Digital_nonAIPE_C : PaymentRuleIntegrationTestBase
    {
        // rule name - "Digital-NonAIPE-NonDPV-C"
        [Test]
        public async Task PaymentRule_NonAIPE_B_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "50000", // irrelevant
                items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 49000 } },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 0% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_NonAIPE_B_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 40000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 13000 }
                },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money - 27400
            receipt.PaymentAmount.ShouldBeEquivalentTo(27400);
        }

        [Test]
        public async Task PaymentRule_NonAIPE_B_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 40000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 46000 }
                },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(-5600);
        }
    }
}

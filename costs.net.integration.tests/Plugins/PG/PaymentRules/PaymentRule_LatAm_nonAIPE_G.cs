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

    public class PaymentRule_LatAm_nonAIPE_G : PaymentRuleIntegrationTestBase
    {
        // rule name - "LatinAmerica-NonAIPE-NonDPV-G"
        [Test]
        public async Task PaymentRule_LatinAmerica_G_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.Latim,
                targetBudget: "12312", // irrelevant
                items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 1000 } },
                contentType: Constants.ContentType.Video
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(500);
        }     

        [Test]
        public async Task PaymentRule_LatinAmerica_G_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.Latim,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 40000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 13000 }
                },
                contentType: Constants.ContentType.Digital
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money - 27000
            receipt.PaymentAmount.ShouldBeEquivalentTo(27000);
        }

        [Test]
        public async Task PaymentRule_LatinAmerica_G_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.Latim,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 40000 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 46000 }
                },
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(-5900);
        }

    }
}

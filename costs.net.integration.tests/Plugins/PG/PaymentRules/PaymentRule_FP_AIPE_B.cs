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

    public class PaymentRule_FP_AIPE_B : PaymentRuleIntegrationTestBase
    {
        // rule name - "FullProduction-AIPE-NonDPV-B"
        [Test]
        public async Task PaymentRule_AIPE_B_AIPE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "50002",
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(25001);
        }

        [Test]
        public async Task PaymentRule_AIPE_B_StillImage_Europe()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "50002",
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(25001);
        }

        [Test]
        public async Task PaymentRule_AIPE_B_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                targetBudget: "5000", // irrelevant
                items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 500000 } },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_B_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 50000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 13000 }
                },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money - 37000
            receipt.PaymentAmount.ShouldBeEquivalentTo(37000);
        }

        [Test]
        public async Task PaymentRule_AIPE_B_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 50000 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 56000 }
                },
                contentType: Constants.ContentType.Video,
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

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

    public class PaymentRule_AAK_AIPE_B : PaymentRuleIntegrationTestBase
    {
        // rule name - "AAK-VideoPostProduction-AIPE-NonDPV-B"
        [Test]
        public async Task PaymentRule_AIPE_AAK_VPP_B_AIPE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "50000",
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 50% of the cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(25000);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_VPP_B_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "50000", // irrelevant
                items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 50002 } },
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // we should have 0.00% of the cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_VPP_B_FP_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 50000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "SOMETHINGELSE", ValueInDefaultCurrency = 300 }},
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 0% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_VPP_B_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 50000 },
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
            // 100% of remaining money - 37400
            receipt.PaymentAmount.ShouldBeEquivalentTo(37400);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_VPP_B_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 50000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 56000 }
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

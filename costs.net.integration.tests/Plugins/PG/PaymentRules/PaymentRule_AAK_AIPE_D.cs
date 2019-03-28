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

    public class PaymentRule_AAK_AIPE_D : PaymentRuleIntegrationTestBase
    {
        // rule name - "AAK-FullProduction-AIPE-NonDPV-D"
        [Test]
        public async Task PaymentRule_AIPE_AAK_D_AIPE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "20000",
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(10000);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5000", // irrelevant
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 5000 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 5000 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 5000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 5000 },
                    new CostLineItemView() { Name = "test", ValueInDefaultCurrency = 5000 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_FP_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 400 },//(incl. insurance)
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 200 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of Production
            // 100% of Insurance
            // 100% of Technical Fee
            // 50% of Post Production
            receipt.PaymentAmount.ShouldBeEquivalentTo(700);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_FP_stage_with_previous_payment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 400 },//(incl. insurance)
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 200 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = 100 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of Production
            // 100% of Insurance
            // 100% of Technical Fee
            // 50% of Post Production
            // -100 
            receipt.PaymentAmount.ShouldBeEquivalentTo(600);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 200 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.TechnicalFee, LineItemTotalCalculatedValue = 100 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money
            receipt.PaymentAmount.ShouldBeEquivalentTo(900);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 200 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.InsuranceTotal, LineItemTotalCalculatedValue = 2000 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(-1000);
        }
    }
}

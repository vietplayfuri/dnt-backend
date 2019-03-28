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

    public class PaymentRule_AAK_AIPE_E : PaymentRuleIntegrationTestBase
    {
        // rule name - "AAK-FullProduction-AIPE-NonDPV-E"
        [Test]
        public async Task PaymentRule_AIPE_AAK_E_AIPE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "60000",
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(30000);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_E_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5000", // irrelevant
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = "test", ValueInDefaultCurrency = 20000 }
                },
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_E_FA_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 20000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 10000 }
                },
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money
            receipt.PaymentAmount.ShouldBeEquivalentTo(90000);
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_E_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 20000 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 20000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 200000 }
                },
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            receipt.PaymentAmount.ShouldBeEquivalentTo(-100000);
        }
    }
}

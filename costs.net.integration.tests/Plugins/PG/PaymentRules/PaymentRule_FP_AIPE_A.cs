namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using plugins;
    using plugins.PG.Models.Stage;

    public class PaymentRules_FP_AIPE_A : PaymentRuleIntegrationTestBase
    {
        // rule name - "FullProduction-AIPE-NonDPV-A"
        [Test]
        public async Task PaymentRule_AIPE_A_AIPE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "500",
                agencyCurrency: "GBP"
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // we should have 50% of the target budget 
            // GBP exchange rate applied - 250/2
            receipt.PaymentAmount.ShouldBeEquivalentTo(125);
        }

        [Test]
        public async Task PaymentRule_AIPE_A_OE_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                targetBudget: "500",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 } }
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 0%
            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_AIPE_A_FP_stage()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                targetBudget: "500",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 200 }, //100% (incl. insurance)
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 100 },//100%
                    new CostLineItemView() { Name= Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },//100%
                    new CostLineItemView() { Name = "SOMETHINGELSE", ValueInDefaultCurrency = 100 },//0%
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 } }//50%
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(350);
        }

        [Test]
        public async Task PaymentRule_AIPE_A_FP_stage_with_previous_payments()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                targetBudget: "500",
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
            // production = 150-100 = 50 * 100%
            // post production = 100 * 50%
            // insurance = 100 * 100%
            // technical fee = 100 * 100%
            // all other costs = 100 * 0%
            // sum = 50 + 50 + 100 + 100 + 0 = 300
            receipt.PaymentAmount.ShouldBeEquivalentTo(300);
        }

        [Test]
        public async Task PaymentRule_AIPE_A_FA_stage_with_previous_payments_with_overpayment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                targetBudget: "2000",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 150 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 100 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 100 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.TargetBudgetTotal, LineItemTotalCalculatedValue = 1000 },
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 1000, LineItemRemainingCost = 1000 }
                }
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // production = 150 * 100%
            // post production = 100 * 100%
            // insurance = 100 * 100%
            // technical fee = 100 * 100%
            // all other costs = 100 * 100%

            // target budget total = 0 - 1000
            // sum = 150 +(-1000) + 100 + 100 + 100 + 100 = -450
            receipt.PaymentAmount.ShouldBeEquivalentTo(-450);
        }
    }
}

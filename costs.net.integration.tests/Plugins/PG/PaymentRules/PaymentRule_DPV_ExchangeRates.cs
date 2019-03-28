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

    public class PaymentRule_DPV_ExchangeRates : PaymentRuleIntegrationTestBase
    {
        [Test]
        public async Task DPV_PaymentRule_NonAIPE_B_FA_stage_with_previous_payments_USD_to_EUR()
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
                productionType: Constants.ProductionType.CgiAnimation,
                agencyCurrency: "GBP",
                dpvCurrency: eurId
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of remaining money - 27400
            // EUR exchange rate applied - 5480
            receipt.PaymentAmount.ShouldBeEquivalentTo(5480);
        }

        [Test]
        public async Task DPV_PaymentRule_NonAIPE_B_FA_stage_with_previous_payments_with_overpayment_USD_to_GBP()
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
                productionType: Constants.ProductionType.CgiAnimation,
                agencyCurrency: "EUR",
                dpvCurrency: gbpId
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // payment calculation according to the rule:
            // 100% of cost total
            // EUR exchange rate applied - -5600 * 0.2
            receipt.PaymentAmount.ShouldBeEquivalentTo(-2800);
        }
    }
}

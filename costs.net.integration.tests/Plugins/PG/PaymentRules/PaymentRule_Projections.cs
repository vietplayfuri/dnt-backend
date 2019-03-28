namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    class PaymentRule_Projections : PaymentRuleIntegrationTestBase
    {
        [Test]
        public async Task PaymentRule_projection()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.Aipe.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "10000",
                agencyCurrency: "USD"
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);

            // Assert
            // we should have 50% of the target budget paid on AIPE and 50% at FA
            //receipt.PaymentAmount.ShouldBeEquivalentTo(250);

            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(5000);
            paymentAmountResult.ProjectedPayments.Count.ShouldBeEquivalentTo(3);
            paymentAmountResult.ProjectedPayments.First(x=>x.StageName == CostStages.FinalActual.ToString()).TotalCostAmountPayment.ShouldBeEquivalentTo(5000);
        }

        [Test]
        public async Task PaymentRule_projection_OE()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "10000",
                agencyCurrency: "USD",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 1500 }, // including prod insurance
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 2000 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 1000 },
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 500 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 2000 },
                }
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);

            // Assert
            // nothing is paid on OE
            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(0);
            paymentAmountResult.ProjectedPayments.Count.ShouldBeEquivalentTo(2);
            // FP Payment = 100% Prod + 100% Insurance + 100% Tech fee + 50% Post Prod + 0 % Other
            // 1000 + 500 + 2000 + 0.5*2000 = 4500
            paymentAmountResult.ProjectedPayments.First(x => x.StageName == CostStages.FirstPresentation.ToString()).TotalCostAmountPayment.ShouldBeEquivalentTo(4500);
        }

        [Test]
        public async Task PaymentRule_projection_OE_with_AIPE_Payment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.OriginalEstimate.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "10000",
                agencyCurrency: "USD",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 1000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 2000 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 1000 },
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 500 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 2000 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.TargetBudgetTotal, LineItemTotalCalculatedValue = 5000 }, // AIPE Payment
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemRemainingCost = 5000 } // carry over from AIPE
                }
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);

            // Assert
            // nothing is paid on OE
            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(0);
            paymentAmountResult.ProjectedPayments.Count.ShouldBeEquivalentTo(2);
            // FP Payment = 100% Prod + 100% Insurance + 100% Tech fee + 50% Post Prod + 0 % Other
            // 1000 + 500 + 2000 + 0.5*2000 = 4500
            // 5000 carried over so 5000 - 4500 = 500 to carry over 
            paymentAmountResult.ProjectedPayments.First(x => x.StageName == CostStages.FirstPresentation.ToString()).TotalCostAmountPayment.ShouldBeEquivalentTo(0);
            // FA Payment = 100% Prod + 100% Insurance + 100% Tech fee + 100% Post Prod + 100 % Other
            // 0 + 0 + 0 + 0.5*2000 + 1000 = 2000
            // 500 carried over so 500 - 2000 = 0 to carry over
            // payment = 2000 - 500 = 1500
            paymentAmountResult.ProjectedPayments.First(x => x.StageName == CostStages.FinalActual.ToString()).TotalCostAmountPayment.ShouldBeEquivalentTo(1500);
        }

        [Test]
        public async Task PaymentRule_AIPE_CarryOver()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "10000",
                agencyCurrency: "USD",
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 1000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 2000 },
                    new CostLineItemView() { TemplateSectionName = "WHATEVERELSE", ValueInDefaultCurrency = 1000 },
                    new CostLineItemView() { Name = Constants.CostSection.InsuranceTotal, ValueInDefaultCurrency = 500 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 2000 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.TargetBudgetTotal, LineItemTotalCalculatedValue = 5000 }, // will not be used in calc
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = 1000 },// 1000 - 1000 = 0
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.PostProduction, LineItemTotalCalculatedValue = 2000}, // 2000-2000 = 0
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemRemainingCost = 2000 } // carry over from AIPE - was 5000 - 1000 - 2000
                }
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);


            // Assert
            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(1500);
            paymentAmountResult.ProjectedPayments.Count.ShouldBeEquivalentTo(0);
        }
        [Test]
        public async Task PgPaymentServices_Has_1_FA_Approved()
        {
            // Arrange
            InitDataForReopenedFA(
                isAipe: true,
                stageKey: CostStages.FinalActual.ToString(),
                budgetRegion: Constants.BudgetRegion.Europe,
                targetBudget: "50000",
                agencyCurrency: "USD",
                previousFAItems: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 12000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Other, ValueInDefaultCurrency = 2000 },
                },
                previousFAPayments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemRemainingCost=-12000, LineItemTotalCalculatedValue = 12000 },
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Other, LineItemTotalCalculatedValue = 2000, LineItemRemainingCost=2000, LineItemFullCost=2000},
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 2000, LineItemFullCost=14000 } // 14000=Production 12000 + Other 2000
                },
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 12000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 7000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Other, ValueInDefaultCurrency = 2000 },
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = 12000 },
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.PostProduction, LineItemTotalCalculatedValue = 7000}, 
                }
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);


            // Assert
            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(7000);
        }
    }
}

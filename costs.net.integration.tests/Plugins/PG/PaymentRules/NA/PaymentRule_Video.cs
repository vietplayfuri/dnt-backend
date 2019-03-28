namespace costs.net.integration.tests.Plugins.PG.PaymentRules.NA
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    // ADC-2237
    public class PaymentRule_Video : PaymentRuleIntegrationTestBase
    {
        public class TestCase
        {
            public static string BudgetRegion => Constants.BudgetRegion.NorthAmerica;
            public static string ContectType => Constants.ContentType.Video;

            public string ProductionType { get; set; }
            public CostStages Stage { get; set; }
            public decimal ProductionCostsAmount { get; set; }
            public decimal ProductionCostsPayedAmount { get; set; }

            public decimal TalentFeesAmount { get; set; }
            public decimal TalentFeesPayedAmount { get; set; }

            public decimal ProductionInsurance { get; set; }
            public decimal InsuranceTotalPayed { get; set; }

            public decimal TechFeeAmount { get; set; }
            public decimal TechFeeAmountPayed { get; set; }

            public decimal PostProductionCostsAmount { get; set; }
            public decimal PostProductionCostsPayedAmount { get; set; }

            public decimal ExpectedPaymentAmount { get; set; }
        }

        private static TestCase[] _fullProduction = {
            // OE Stage
            // (100% Production cost total - 100% Talent fees) / 2 + (100% of talent fees) + (100% of insurance) + (100% of Tech fee)
            new TestCase
            {
                Stage = CostStages.OriginalEstimate,
                ProductionType = Constants.ProductionType.FullProduction,
                ProductionCostsAmount = 1000,
                TalentFeesAmount = 500,
                ProductionInsurance = 200,
                TechFeeAmount = 100,
                // (1000 - 500 - 200) * 0.5 + 500 * 1.0 + 200 * 1.0 + 100 * 1.0 = 950
                ExpectedPaymentAmount = 950
            },
            // FP Stage
            // (100% Remaining Prod cost) + (50% Post prod cost)
            new TestCase
            {
                Stage = CostStages.FirstPresentation,
                ProductionType = Constants.ProductionType.FullProduction,
                ProductionCostsAmount = 1000,
                TalentFeesAmount = 500,
                ProductionInsurance = 200,
                TechFeeAmount = 100,
                PostProductionCostsAmount = 700,
                // Payed
                ProductionCostsPayedAmount = 650,
                TalentFeesPayedAmount = 500,
                InsuranceTotalPayed = 200,
                TechFeeAmountPayed = 100,
                // (1000 - 200 - 500 - 150) * 1.0 + 700 * 0.5 = 500
                ExpectedPaymentAmount = 500
            }
            //,
            //// FA Stage
            //// 100% Total Remaining costs
            //new TestCase
            //{
            //    Stage = CostStages.FinalActual,
            //    ProductionType = Constants.ProductionType.FullProduction,
            //    ProductionCostsAmount = 1000,
            //    TalentFeesAmount = 500,
            //    ProductionInsurance = 200,
            //    TechFeeAmount = 100,
            //    PostProductionCostsAmount = 700,
            //    // Payed
            //    ProductionCostsPayedAmount = 1000,
            //    TalentFeesPayedAmount = 500,
            //    InsuranceTotalPayed = 200,
            //    TechFeeAmountPayed = 100,
            //    PostProductionCostsPayedAmount = 350,
            //    // Remaining is only 350 of PostProduction costs => 350 * 1.0 = 350
            //    ExpectedPaymentAmount = 350
            //}
        };

        [Test]
        [TestCaseSource(nameof(_fullProduction))]
        public async Task PaymentRule_Europe_Video(TestCase testCase)
        {
            // Arrange
            InitData(
                isAipe: false,
                stageKey: testCase.Stage.ToString(),
                budgetRegion: TestCase.BudgetRegion,
                items: new List<CostLineItemView>
                {
                    new CostLineItemView { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = testCase.ProductionCostsAmount },
                    new CostLineItemView { Name = Constants.CostSection.TalentFees, ValueInDefaultCurrency = testCase.TalentFeesAmount },
                    new CostLineItemView { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = testCase.ProductionInsurance },
                    new CostLineItemView { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = testCase.TechFeeAmount },
                    new CostLineItemView { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = testCase.PostProductionCostsAmount }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = testCase.ProductionCostsPayedAmount },
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.TalentFees, LineItemTotalCalculatedValue = testCase.TalentFeesPayedAmount },
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.InsuranceTotal, LineItemTotalCalculatedValue = testCase.InsuranceTotalPayed },
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.TechnicalFee, LineItemTotalCalculatedValue = testCase.TechFeeAmountPayed },
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.PostProduction, LineItemTotalCalculatedValue = testCase.PostProductionCostsPayedAmount },

                },
                contentType: TestCase.ContectType,
                productionType: testCase.ProductionType
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            receipt.PaymentAmount.ShouldBeEquivalentTo(testCase.ExpectedPaymentAmount);
        }
    }
}

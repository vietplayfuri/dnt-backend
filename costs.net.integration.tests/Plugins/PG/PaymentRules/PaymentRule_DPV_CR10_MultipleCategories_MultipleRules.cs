namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    public class PaymentRule_DPV_CR10_MultipleCategories_MultipleRules : PaymentRuleIntegrationTestBase
    {
        [Test]
        [TestCase("OriginalEstimate", "0", "0")]
        [TestCase("FirstPresentation", "15000", "15000")]
        public async Task PaymentRule_projection_0_OE_100_FP(string stageKey, string strTotalCost, string strAmountPayment)
        {
            // Arrange
            const string audioVendorCategory = "Video company";
            _vendorWithMultipleRulesId = await CreateVendorVideoProductionRule();

            InitData(
                isAipe: false,
                stageKey: stageKey,
                budgetRegion: Constants.BudgetRegion.Europe,
                contentType: Constants.ContentType.Video,
                targetBudget: "50000",
                agencyCurrency: "USD",
                dpvCurrency: usdId,
                dpvId: _vendorWithMultipleRulesId,
                vendorCategory: audioVendorCategory,
                items: new List<CostLineItemView>() {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 15000 }, // including prod insurance
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 2000 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Other, ValueInDefaultCurrency = 10000 },
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 0 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 0 },
                }
            );

            // Act
            var paymentAmountResult = await _paymentService.CalculatePaymentAmount(_costStageRevisionId);

            // Assert
            paymentAmountResult.TotalCostAmount.ShouldBeEquivalentTo(decimal.Parse(strTotalCost));
            paymentAmountResult.TotalCostAmountPayment.ShouldBeEquivalentTo(decimal.Parse(strAmountPayment));
        }

        [Test]
        public async Task DPV_Overrides_PaymentRules_MultipleCategories_MultipleRules_OE()
        {
            const string audioVendorCategory = "Audio company";

            if (!_vendorWithMultipleRulesId.HasValue)
            {
                _vendorWithMultipleRulesId = await CreateVendor();
            }
            InitData(
                false,
                CostStages.OriginalEstimate.ToString(),
                Constants.BudgetRegion.Europe,
                new List<CostLineItemView>
                {
                    new CostLineItemView { Name = Constants.CostSection.CostTotal, ValueInDefaultCurrency = 11000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 0 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                agencyCurrency: "USD",
                dpvCurrency: usdId,
                dpvId: _vendorWithMultipleRulesId,
                vendorCategory: audioVendorCategory
            );
            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // Remaining money - 11000.00
            // 40% of remaining money - 4400.00
            receipt.PaymentAmount.ShouldBeEquivalentTo(4400.00m);
        }

        [Test]
        public async Task DPV_Overrides_PaymentRules_MultipleCategories_MultipleRules_FA()
        {
            const string audioVendorCategory = "Audio company";
            if (!_vendorWithMultipleRulesId.HasValue)
            {
                _vendorWithMultipleRulesId = await CreateVendor();
            }
            InitData(
                false,
                CostStages.FinalActual.ToString(),
                Constants.BudgetRegion.Europe,
                new List<CostLineItemView>
                {
                    new CostLineItemView { Name = Constants.CostSection.CostTotal, ValueInDefaultCurrency = 11000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 4400 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                agencyCurrency: "USD",
                dpvCurrency: usdId,
                dpvId: _vendorWithMultipleRulesId,
                vendorCategory: audioVendorCategory
            );
            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // Remaining money - 6600.00
            // 70% of remaining money - 4620.00
            receipt.PaymentAmount.ShouldBeEquivalentTo(4620.00);
        }

        [Test]
        public async Task DPV_Overrides_PaymentRules_MultipleCategories_MultipleRules_FA_NoMatchingCustomRule()
        {
            const string audioVendorCategory = "Random company";
            if (!_vendorWithMultipleRulesId.HasValue)
            {
                _vendorWithMultipleRulesId = await CreateVendor();
            }
            InitData(
                false,
                CostStages.FinalActual.ToString(),
                Constants.BudgetRegion.Europe,
                new List<CostLineItemView>
                {
                    new CostLineItemView { Name = Constants.CostSection.CostTotal, ValueInDefaultCurrency = 11000 }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 4400 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                agencyCurrency: "USD",
                dpvCurrency: usdId,
                dpvId: _vendorWithMultipleRulesId,
                vendorCategory: audioVendorCategory
            );
            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // Remaining money - 6600.00
            // 100% of remaining money - 6600.00
            receipt.PaymentAmount.ShouldBeEquivalentTo(6600);
        }
    }
}

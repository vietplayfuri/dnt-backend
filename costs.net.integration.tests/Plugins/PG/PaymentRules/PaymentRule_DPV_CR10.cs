namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Rules;
    using plugins.PG.Models.Stage;

    public class PaymentRule_DPV_CR10 : PaymentRuleIntegrationTestBase
    {
        [Test]
        public async Task DPV_Overrides_PaymentRules()
        {
            var vendorId = await InitDpvData(
                "SapVendorCode1234111",
                "Vendor 1",
                false,
                Constants.BudgetRegion.China,
                Constants.ContentType.Audio,
                Constants.ProductionType.PostProductionOnly,
                "Post Production company",
                1000,
                new PgPaymentRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgPaymentRuleDefinitionSplit
                        {
                            FASplit = (decimal) 0.7,
                            CostTotalName = Constants.CostSection.CostTotal
                        }
                    }
                });

            InitData(
                false,
                CostStages.FinalActual.ToString(),
                Constants.BudgetRegion.China,
                new List<CostLineItemView>
                {
                    new CostLineItemView { Name = Constants.CostSection.CostTotal, ValueInDefaultCurrency = (decimal) 14000.00 }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 13000 }
                },
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.PostProductionOnly,
                agencyCurrency: "GBP",
                dpvCurrency: gbpId,
                dpvId: vendorId
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // Remaining money - 1000.00
            // 70% of remaining money - 700.00
            // EUR exchange rate applied - 350.00
            receipt.PaymentAmount.ShouldBeEquivalentTo(350.00);
        }

        [Test]
        public async Task DPV_Overrides_PaymentRules_For_AllContecntTypes()
        {
            var vendorId = await InitDpvData(
                "SapCode12356",
                "Vendor 2",
                false,
                Constants.BudgetRegion.China,
                "All",
                Constants.ProductionType.PostProductionOnly,
                "Post Production company",
                1000,
                new PgPaymentRuleDefinition
                {
                    Splits = new[]
                    {
                        new PgPaymentRuleDefinitionSplit
                        {
                            FASplit = (decimal) 0.6,
                            CostTotalName = Constants.CostSection.CostTotal
                        }
                    }
                });

            InitData(
                false,
                CostStages.FinalActual.ToString(),
                Constants.BudgetRegion.China,
                new List<CostLineItemView>
                {
                    new CostLineItemView { Name = Constants.CostSection.CostTotal, ValueInDefaultCurrency = (decimal) 14000.00 }
                },
                payments: new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal { LineItemTotalType = Constants.CostSection.CostTotal, LineItemTotalCalculatedValue = 13000 }
                },
                contentType: Constants.ContentType.Digital,
                productionType: Constants.ProductionType.PostProductionOnly,
                agencyCurrency: "GBP",
                dpvCurrency: gbpId,
                dpvId: vendorId
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // Remaining money - 1000.00
            // 60% of remaining money - 600.00
            // EUR exchange rate applied - 300.00
            receipt.PaymentAmount.ShouldBeEquivalentTo(300.00);
        }

        private async Task<Guid?> InitDpvData(
            string sapvendorNumber,
            string vendorName = "Test vendor 1",
            bool isAipe = false,
            string budgetRegion = "AAK (Asia)",
            string contentType = Constants.ContentType.Video,
            string productionType = Constants.ProductionType.FullProduction,
            string category = "Production company",
            decimal total = 0,
            PgPaymentRuleDefinition ruleDefinition = null
        )
        {
            var rule = GetRule(vendorName, isAipe, budgetRegion, contentType, productionType, total, ruleDefinition);

            var vendor = new Vendor
            {
                Name = vendorName,
                SapVendor = sapvendorNumber,
                Categories = new List<VendorCategory>()
            };
            var vendorCategory = new VendorCategory
            {
                Name = category,
                Vendor = vendor,
                HasDirectPayment = true,
                Currency = new Currency { Id = usdId, Code = "USD", Description = "USD", Symbol = "s" },
            };
            var vendorRule = new VendorRule { Rule = rule, VendorCategory = vendorCategory };
            EFContext.VendorRule.Add(vendorRule);

            await EFContext.SaveChangesAsync();
            return vendorRule.VendorCategory.VendorId;
        }
    }
}

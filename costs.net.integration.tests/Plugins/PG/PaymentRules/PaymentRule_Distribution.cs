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

    public class PaymentRule_Distribution: PaymentRuleIntegrationTestBase
    {
        [Test]
        public async Task PaymentRule_Distribution_OE()
        {
            InitData(
              stageKey: CostStages.OriginalEstimate.ToString(),
              budgetRegion: Constants.BudgetRegion.Europe,
              targetBudget: "12312", // irrelevant
              items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 12312 } },              
              costType: CostType.Trafficking
            );

            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            receipt.PaymentAmount.ShouldBeEquivalentTo(0);
        }

        [Test]
        public async Task PaymentRule_Distribution_FA()
        {
            InitData(
              stageKey: CostStages.FinalActual.ToString(),
              budgetRegion: Constants.BudgetRegion.Europe,
              targetBudget: "12312", // irrelevant
              items: new List<CostLineItemView>() { new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 12312 } },
              costType: CostType.Trafficking
            );

            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);
            
            receipt.PaymentAmount.ShouldBeEquivalentTo(12312);
        }
    }
}

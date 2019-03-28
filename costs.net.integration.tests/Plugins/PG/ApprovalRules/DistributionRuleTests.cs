
namespace costs.net.integration.tests.Plugins.PG.ApprovalRules
{
    using System;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System.Threading.Tasks;
    using plugins.PG.Models.Stage;

    public class DistributionRuleTests :ApprovalRuleIntegrationTestBase
    {
        [Test]
        public async Task Distribution_Approvals_OE()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, string.Empty , 9000, Constants.BudgetRegion.China, CostType.Trafficking);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task Distribution_Approvals_FA()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, string.Empty, 9000, Constants.BudgetRegion.China, CostType.Trafficking, null,null, CostStages.FinalActual);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }
    }
}

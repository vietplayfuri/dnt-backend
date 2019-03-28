namespace costs.net.integration.tests.Plugins.PG.ApprovalRules
{
    using System;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    public class CommonApproverRuleTests : ApprovalRuleIntegrationTestBase
    {
        [Test]
        public async Task TestAipeApproversInOE()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 1, Constants.BudgetRegion.IndiaAndMiddleEastAfrica, CostType.Production,
                Constants.ProductionType.FullProduction);
            SetPreviousRevision(CostStages.Aipe);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }
    }
}
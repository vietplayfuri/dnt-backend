namespace costs.net.integration.tests.Plugins.PG.ApprovalRules
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class JapanRuleTests : ApprovalRuleIntegrationTestBase
    {

        [Test]
        public async Task TestJapanRule1a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 9000, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule1b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 9999, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule1c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 1, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule1d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 9999, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule1e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 9999, Constants.BudgetRegion.Japan, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule1f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 9999, Constants.BudgetRegion.Japan, CostType.Buyout, Constants.ProductionType.FullProduction, false);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 11000, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 11000, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 11000, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 11000, Constants.BudgetRegion.Japan, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 11000, Constants.BudgetRegion.Japan, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestJapanRule2f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 11000, Constants.BudgetRegion.Japan, CostType.Buyout, Constants.ProductionType.FullProduction, false);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }
    }
}

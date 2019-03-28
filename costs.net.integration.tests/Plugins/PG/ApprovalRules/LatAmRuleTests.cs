namespace costs.net.integration.tests.Plugins.PG.ApprovalRules
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class LatAmRuleTests : ApprovalRuleIntegrationTestBase
    {
        /// <summary>
        /// ADC-1132
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestLatAmRule1a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 110000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeTrue();
        }

        [Test]
        public async Task TestLatAmRule1b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 60000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestLatAmRule1c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 56000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestLatAmRule1d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 60000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestLatAmRule1e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 60000, Constants.BudgetRegion.Latim, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestLatAmRule1f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 1250000, Constants.BudgetRegion.Latim, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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

        // LatAmApprovalRule-ADC-1132-B-1
        public async Task TestLatAmRule2a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 60000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        public async Task TestLatAmRule2b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 40000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        public async Task TestLatAmRule2c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 40000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        // LatAmApprovalRule-ADC-1132-B-2
        public async Task TestLatAmRule2d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        public async Task TestLatAmRule2e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.Latim, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
        }

        //LatAmApprovalRule-ADC-1132-C-1
        public async Task TestLatAmRule2f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 500, Constants.BudgetRegion.Latim, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }
    }
}

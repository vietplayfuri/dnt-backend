namespace costs.net.integration.tests.Plugins.PG.ApprovalRules
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class EuropeRuleTests : ApprovalRuleIntegrationTestBase
    {
        /// <summary>
        /// ADC-660
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestEuropeRule1a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 110000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule1b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 110000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule1c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 55000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule1d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 55000, Constants.BudgetRegion.Europe, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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
        public async Task TestEuropeRule1e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 55000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule1f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 125000, Constants.BudgetRegion.Europe, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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
        public async Task TestEuropeRule1g()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 125000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        /// <summary>
        /// ADC-661
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestEuropeRule2a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 95000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule2b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 95000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule2c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 45000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestEuropeRule2d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 45000, Constants.BudgetRegion.Europe, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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
        public async Task TestEuropeRule2e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 11000, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
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

        /// <summary>
        /// ADC-662
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestEuropeRule3a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 9999, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }
        [Test]
        public async Task TestEuropeRule3b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 9999, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestEuropeRule3c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 9999, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestEuropeRule3d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 4900, Constants.BudgetRegion.Europe, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestEuropeRule3e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 9999, Constants.BudgetRegion.Europe, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }
    }
}

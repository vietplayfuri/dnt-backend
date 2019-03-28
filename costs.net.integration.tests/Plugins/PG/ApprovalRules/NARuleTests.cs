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
    public class NARuleTests : ApprovalRuleIntegrationTestBase
    {
        /// <summary>
        /// ADC-1131
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestNANonCycloneRule1a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 110000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestNANonCycloneRule1b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestNANonCycloneRule1c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 56000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestNANonCycloneRule1d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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
        public async Task TestNANonCycloneRule1e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, string.Empty, 100001, Constants.BudgetRegion.NorthAmerica, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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
        public async Task TestNANonCycloneRule1f()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 1250000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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

        // NAApprovalRule-ADC-1131-B-1
        public async Task TestNANonCycloneRule2a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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

        public async Task TestNANonCycloneRule2b()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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

        public async Task TestNANonCycloneRule2c()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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

        // NAApprovalRule-ADC-1131-B-2
        public async Task TestNANonCycloneRule2d()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
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

        public async Task TestNANonCycloneRule2e()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Buyout, Constants.ProductionType.FullProduction, true);
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

        //NAApprovalRule-ADC-1131-C-1
        public async Task TestNANonCycloneRule3a()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 500, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        /// <summary>
        /// ADC-1130
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestNACycloneRule1a()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 110000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule1b()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule1c()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 56000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule1d()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule1e()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, string.Empty, 100001, Constants.BudgetRegion.NorthAmerica, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule1f()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 1250000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().NotContain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        /// <summary>
        /// ADC-1130 - B
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestNACycloneRule2a()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 60000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule2b()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Audio, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule2c()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Photography, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        // NAApprovalRule-ADC-1130-B-2
        [Test]
        public async Task TestNACycloneRule2d()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNACycloneRule2e()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Digital, 40000, Constants.BudgetRegion.NorthAmerica, CostType.Buyout, Constants.ProductionType.FullProduction, true);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
            var ipmApproval = approvals[0];
            ipmApproval.Type.Should().Be(ApprovalType.IPM);
            ipmApproval.ValidBusinessRoles.Should().Contain("Integrated Production Manager");
            ipmApproval.ValidBusinessRoles.Should().Contain("Cost Consultant");
            approvals[1].Type.Should().Be(ApprovalType.Brand);
            approvals[1].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        //NAApprovalRule-ADC-1130-C-1
        [Test]
        public async Task TestNACycloneRule3a()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, Constants.ContentType.Video, 500, Constants.BudgetRegion.NorthAmerica, CostType.Production, Constants.ProductionType.FullProduction);
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Production, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
            approvals[0].Members.Any(x => x.IsExternal).Should().BeFalse();
        }

        [Test]
        public async Task TestNANonCycloneTrafficking()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId,
                Constants.ContentType.Audio,
                10,
                Constants.BudgetRegion.NorthAmerica,
                CostType.Trafficking,
                Constants.ProductionType.FullProduction
            );

            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Trafficking, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            var approval = approvals[0];

            approval.Type.Should().Be(ApprovalType.Brand);
            approval.Members.Any(x => x.IsExternal).Should().BeTrue();
        }

        [TestCase(10)]
        [TestCase(100000000)]
        public async Task TestNAContractCost(int costTotal)
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId,
                Constants.UsageBuyoutType.Contract,
                costTotal,
                Constants.BudgetRegion.NorthAmerica,
                CostType.Buyout,
                Constants.ProductionType.FullProduction,true);

            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();

            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            approvals[0].Type.Should().Be(ApprovalType.Brand);
        }

        [Test]
        public async Task TestNABuyoutCost()
        {
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId,
                Constants.UsageBuyoutType.Buyout,
                1000110000,
                Constants.BudgetRegion.NorthAmerica,
                CostType.Buyout,
                Constants.ProductionType.FullProduction);

            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();

            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Buyout, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(2);
        }

        //ADC-2075 
        [Test]
        public async Task TestNACycloneTrafficking()
        {
            SetupCycloneAgencyMock();
            var latestRevisionId = Guid.NewGuid();
            var stageDetails = BuildStageDetails(latestRevisionId, 
                Constants.ContentType.Audio, 
                10, 
                Constants.BudgetRegion.NorthAmerica, 
                CostType.Trafficking, 
                Constants.ProductionType.FullProduction
                );

            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvals = (await _pgCostBuilder.GetApprovals(CostType.Trafficking, stageDetails, userId, latestRevisionId, costId)).ToArray();

            approvals.Length.Should().Be(1);
            var approval = approvals[0];

            approval.Type.Should().Be(ApprovalType.Brand);
            approval.Members.Any(x => x.IsExternal).Should().BeFalse();
        }

    }
}

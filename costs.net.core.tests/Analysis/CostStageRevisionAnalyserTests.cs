using System;
using System.Collections.Generic;
using System.Linq;
using costs.net.core.Analysis;
using costs.net.dataAccess.Entity;
using FluentAssertions;
using NUnit.Framework;

namespace costs.net.core.tests.Analysis
{
    [TestFixture]
    public class CostStageRevisionAnalyserTests
    {
        #region GetRemovedApprovers tests

        [Test]
        public void GetRemovedApprovers_EmptyCostStageRevision()
        {
            //Arrange
            var x = new CostStageRevision();
            var y = new CostStageRevision();
            var target = new CostStageRevisionAnalyser();

            //Act
            var result = target.GetRemovedApprovers(x, y);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetRemovedApprovers_HasNoDifferences()
        {
            //Arrange
            var x = new CostStageRevision();
            var y = new CostStageRevision();
            var xApproval1 = new Approval();
            var xApproval2 = new Approval();
            var xApprovalMember1 = new ApprovalMember();
            var xApprovalMember2 = new ApprovalMember();
            var xCostUser1 = new CostUser();
            var xCostUser2 = new CostUser();

            var target = new CostStageRevisionAnalyser();

            x.Approvals = new List<Approval>();
            y.Approvals = new List<Approval>();
            xApproval1.ApprovalMembers = new List<ApprovalMember>();
            xApproval2.ApprovalMembers = new List<ApprovalMember>();

            xApproval1.ApprovalMembers.Add(xApprovalMember1);
            xApproval2.ApprovalMembers.Add(xApprovalMember2);
            xApprovalMember1.CostUser = xCostUser1;
            xApprovalMember2.CostUser = xCostUser2;

            xCostUser1.Id = Guid.NewGuid();
            xCostUser2.Id = Guid.NewGuid();

            x.Approvals.Add(xApproval1);
            x.Approvals.Add(xApproval2);
            y.Approvals.Add(xApproval1);
            y.Approvals.Add(xApproval2);

            //Act
            var result = target.GetRemovedApprovers(x, y);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetRemovedApprovers_InYAndNotInX()
        {
            //Arrange
            var x = new CostStageRevision();
            var y = new CostStageRevision();
            var xApproval1 = new Approval();
            var xApproval2 = new Approval();
            var xApprovalMember1 = new ApprovalMember();
            var xApprovalMember2 = new ApprovalMember();
            var xCostUser1 = new CostUser();
            var xCostUser2 = new CostUser();

            var target = new CostStageRevisionAnalyser();

            x.Approvals = new List<Approval>();
            y.Approvals = new List<Approval>();
            xApproval1.ApprovalMembers = new List<ApprovalMember>();
            xApproval2.ApprovalMembers = new List<ApprovalMember>();

            xApproval1.ApprovalMembers.Add(xApprovalMember1);
            xApproval2.ApprovalMembers.Add(xApprovalMember2);
            xApprovalMember1.CostUser = xCostUser1;
            xApprovalMember2.CostUser = xCostUser2;

            xCostUser1.Id = Guid.NewGuid();
            xCostUser2.Id = Guid.NewGuid();

            x.Approvals.Add(xApproval1);

            y.Approvals.Add(xApproval1);
            y.Approvals.Add(xApproval2);

            //Act
            var result = target.GetRemovedApprovers(x, y);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetRemovedApprovers_HasRemovedApprovers()
        {
            //Arrange
            var x = new CostStageRevision();
            var y = new CostStageRevision();
            var xApproval1 = new Approval();
            var xApproval2 = new Approval();
            var xApprovalMember1 = new ApprovalMember();
            var xApprovalMember2 = new ApprovalMember();
            var xCostUser1 = new CostUser();
            var removedCostUser = new CostUser();

            var target = new CostStageRevisionAnalyser();

            x.Approvals = new List<Approval>();
            y.Approvals = new List<Approval>();
            xApproval1.ApprovalMembers = new List<ApprovalMember>();
            xApproval2.ApprovalMembers = new List<ApprovalMember>();

            xApproval1.ApprovalMembers.Add(xApprovalMember1);
            xApproval2.ApprovalMembers.Add(xApprovalMember2);
            xApprovalMember1.CostUser = xCostUser1;
            xApprovalMember2.CostUser = removedCostUser;

            xCostUser1.Id = Guid.NewGuid();
            removedCostUser.Id = Guid.NewGuid();

            x.Approvals.Add(xApproval1);
            x.Approvals.Add(xApproval2);
            y.Approvals.Add(xApproval1);

            //Act
            var result = target.GetRemovedApprovers(x, y);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var removedApprover = result.First();
            removedApprover.Should().NotBeNull();
            removedApprover.CostUser.Should().NotBeNull();
            removedApprover.CostUser.Should().Be(removedCostUser);
        }

        #endregion //GetRemovedApprovers tests
    }
}

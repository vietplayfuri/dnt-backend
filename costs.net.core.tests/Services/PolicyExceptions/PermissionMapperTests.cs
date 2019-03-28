
namespace costs.net.core.tests.Services.PolicyExceptions
{
    using System.Linq;
    using core.Services.PolicyExceptions;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionMapperTests
    {
        [Test]
        public void Null_PermissionSet_Returns_Empty()
        {
            //Arrange
            var expected = 0;
            PermissionSet permissionSet = null;
            var status = PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expected);
        }

        [Test]
        public void AllFalse_PermissionSet_Returns_Empty()
        {
            //Arrange
            var expected = 0;
            var status = PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = false,
                CanEdit = false,
                CanView = false
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expected);
        }

        [Test]
        public void CanApprove_PermissionSet_Returns_ApproveMapping()
        {
            //Arrange
            var expectedCount = 1;
            var expectedKey = "approve";
            var status = PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = true,
                CanEdit = false,
                CanView = false
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Key.Should().Be(expectedKey);
        }

        [Test]
        public void CanApprove_ForApprovedPolicyException_DoesNot_Return_ApproveMapping()
        {
            //Arrange
            var expectedCount = 0;
            var status = PolicyExceptionStatus.Approved;
            var costStatus = CostStageRevisionStatus.Approved;
            var permissionSet = new PermissionSet
            {
                CanApprove = true,
                CanEdit = false,
                CanView = false
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public void CanEdit_PermissionSet_Returns_EditMappings()
        {
            //Arrange
            var expectedCount = 2;
            var expectedKey = "edit";
            var status = dataAccess.Entity.PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = false,
                CanEdit = true,
                CanView = false
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Key.Should().Be(expectedKey);
        }

        [Test]
        public void CanEdit_ForApprovedPolicyException_DoesNot_Return_EditMappings()
        {
            //Arrange
            var expectedCount = 0;
            var status = PolicyExceptionStatus.Approved;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = false,
                CanEdit = true,
                CanView = false
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public void CanView_PermissionSet_Returns_ViewMapping()
        {
            //Arrange
            var expectedCount = 1;
            var expectedKey = "view";
            var status = PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = false,
                CanEdit = false,
                CanView = true
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Key.Should().Be(expectedKey);
        }

        [Test]
        public void AllPerms_PermissionSet_Returns_ManyMappings()
        {
            //Arrange
            var expectedCount = 4;
            var expectedKey = "approve";
            var status = PolicyExceptionStatus.PendingApproval;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = true,
                CanEdit = true,
                CanView = true
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Key.Should().Be(expectedKey);
        }

        [Test]
        public void AllPerms_ForApprovedPolicyException_Returns_ViewMappingOnly()
        {
            //Arrange
            var expectedCount = 1;
            var expectedKey = "view";
            var status = PolicyExceptionStatus.Approved;
            var costStatus = CostStageRevisionStatus.Draft;
            var permissionSet = new PermissionSet
            {
                CanApprove = true,
                CanEdit = true,
                CanView = true
            };
            var target = new PermissionMapper();

            //Act
            var result = target.GetMappings(permissionSet, status, costStatus);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Key.Should().Be(expectedKey);
        }
    }
}

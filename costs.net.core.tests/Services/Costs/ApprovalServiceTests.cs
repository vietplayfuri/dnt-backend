namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.Payments;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Serilog;
    using core.Models;
    using core.Models.ACL;
    using core.Models.User;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using core.Models.Payments;
    using core.Services.ActivityLog;
    using core.Services.PolicyExceptions;

    public class ApprovalServiceTests
    {
        public abstract class ApprovalServiceTest
        {
            private Mock<ICostStatusService> _costStatusServiceMock;
            private Mock<IEventService> _eventServiceMock;
            private Mock<IPolicyExceptionsService> _policyExceptionsServiceMock;
            private Mock<IActivityLogService> _activityLogServiceMock;

            protected EFContext EFContext;
            protected Mock<ICostStageRevisionPermissionService> PermissionServiceMock;
            protected Mock<ILogger> LoggerMock;
            protected ApprovalService ApprovalService;
            protected UserIdentity User;
            protected CostUser CostUser;

            [SetUp]
            public void Setup()
            {
                EFContext = EFContextFactory.CreateInMemoryEFContext();
                _costStatusServiceMock = new Mock<ICostStatusService>();
                PermissionServiceMock = new Mock<ICostStageRevisionPermissionService>();
                LoggerMock = new Mock<ILogger>();
                _eventServiceMock = new Mock<IEventService>();
                _policyExceptionsServiceMock = new Mock<IPolicyExceptionsService>();
                _activityLogServiceMock = new Mock<IActivityLogService>();

                User = new UserIdentity
                {
                    Email = "e@mail.com",
                    AgencyId = Guid.NewGuid(),
                    Id = Guid.NewGuid()
                };
                CostUser = new CostUser
                {
                    Id = User.Id,
                    Email = User.Email,
                    ParentId = User.AgencyId
                };

                var brandUser = new UserIdentity
                {
                    Email = "costs.admin@adstream.com",
                    AgencyId = Guid.NewGuid(),
                    Id = Guid.NewGuid()
                };
                var brandCostUser = new CostUser
                {
                    Id = brandUser.Id,
                    Email = brandUser.Email,
                    ParentId = brandUser.AgencyId
                };

                EFContext.CostUser.AddRange(CostUser, brandCostUser);
                EFContext.SaveChanges();

                var pgPaymentService = new Mock<IPaymentService>();
                pgPaymentService.Setup(x => x.CalculatePaymentAmount(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync((PaymentAmountResult)null);

                ApprovalService = new ApprovalService(
                    EFContext,
                    _costStatusServiceMock.Object,
                    PermissionServiceMock.Object,
                    LoggerMock.Object,                   
                    _eventServiceMock.Object,
                    _policyExceptionsServiceMock.Object,
                    _activityLogServiceMock.Object
                );
            }

            protected Cost MockCost()
            {
                var costId = Guid.NewGuid();
                var costStageId = Guid.NewGuid();
                var costStageRevisionId = Guid.NewGuid();
                var costStage = new CostStage
                {
                    Id = costStageId
                };
                var costStageRevision = new CostStageRevision
                {
                    Id = costStageRevisionId,
                    CostStage = costStage,
                    CostStageId = costStageId
                };
                var cost = new Cost
                {
                    Id = costId,
                    Status = CostStageRevisionStatus.Draft,
                    LatestCostStageRevisionId = costStageRevisionId,
                    LatestCostStageRevision = costStageRevision
                };

                EFContext.Cost.Add(cost);
                EFContext.SaveChanges();

                return cost;
            }
        }

        [TestFixture]
        public class SubmitCostShould : ApprovalServiceTest
        {
            [Test]
            public async Task GrantAccessToApprovers()
            {
                // Arrange
                var cost = MockCost();

                var techApprover = new ApprovalMember
                {
                    Id = Guid.NewGuid(),
                    MemberId = Guid.NewGuid(),
                    CostUser = new CostUser
                    {
                        FullName = "Test"
                    }
                };
                var approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalMember>
                        {
                            techApprover
                        }
                    }
                };
                EFContext.Approval.AddRange(approvals);
                EFContext.SaveChanges();

                // Act
                await ApprovalService.SubmitApprovals(cost, CostUser, approvals, BuType.Pg);

                // Assert
                PermissionServiceMock.Verify(p => p.GrantApproverAccess(CostUser.Id, cost.Id, It.IsAny<IEnumerable<CostUser>>(), It.IsAny<BuType>()));
            }

            [Test]
            public void ThrowException_WhenApprovalsCollectionIsNull()
            {
                // Arrange
                var cost = MockCost();

                // Act
                // Assert
                ApprovalService.Awaiting(a => a.SubmitApprovals(cost, CostUser, null, BuType.Pg)).ShouldThrow<ArgumentNullException>();
            }

            [Test]
            public void ThrowException_WhenCountOfApprovalsIsMoreThenTwo()
            {
                // Arrange
                var cost = MockCost();
                var approvas = new bool[3].Select(i => new Approval()).ToList();

                // Act
                // Assert
                ApprovalService.Awaiting(a => a.SubmitApprovals(cost, CostUser, approvas, BuType.Pg)).ShouldThrow<InvalidOperationException>();
            }

            [Test]
            public async Task ThrowException_WhenNoApprovalsShouldLogWarning()
            {
                // Arrange
                var cost = MockCost();
                var approvas = new List<Approval>();

                // Act
                await ApprovalService.SubmitApprovals(cost, CostUser, approvas, BuType.Pg);

                // Assert
                LoggerMock.Verify(l => l.Warning(It.IsAny<string>()), Times.Once);
            }

            [Test]
            public void ThrowException_WhenNoApproversInApproval()
            {
                // Arrange
                var cost = MockCost();
                var approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalMember>()
                    }
                };

                // Act
                // Assert
                ApprovalService.Awaiting(a => a.SubmitApprovals(cost, CostUser, approvals, BuType.Pg)).ShouldThrow<Exception>();
            }

            [Test]
            public void NotThrowException_WhenNotExternalPurchaseAndNoRequisitionersInApproval()
            {
                // Arrange
                var cost = MockCost();
                cost.IsExternalPurchases = false;
                EFContext.SaveChanges();

                var approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalMember> { new ApprovalMember() }
                    },
                    new Approval
                    {
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalMember> { new ApprovalMember() },
                        Requisitioners = new List<Requisitioner>()
                    }
                };

                // Act
                // Assert
                ApprovalService.Awaiting(a => a.SubmitApprovals(cost, CostUser, approvals, BuType.Pg)).ShouldNotThrow<Exception>();
            }

            [Test]
            public void ThrowException_WhenExternalPurchaseAndNoRequisitionersInApproval()
            {
                // Arrange
                var cost = MockCost();
                cost.IsExternalPurchases = true;
                EFContext.SaveChanges();

                var approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalMember> { new ApprovalMember() }
                    },
                    new Approval
                    {
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalMember> { new ApprovalMember() },
                        Requisitioners = new List<Requisitioner>()
                    }
                };

                // Act
                // Assert
                ApprovalService.Awaiting(a => a.SubmitApprovals(cost, CostUser, approvals, BuType.Pg)).ShouldThrow<Exception>();
            }
        }

        [TestFixture]
        public class ApproveCostShould : ApprovalServiceTest
        {
            [Test]
            public async Task SetStatusToApprove()
            {
                // Arrange
                const ApprovalType approvalType = ApprovalType.IPM;
                var approverRoleId = Guid.NewGuid();

                var cost = MockCost();

                var approval = new Approval
                {
                    CostStageRevisionId = cost.LatestCostStageRevision.Id,
                    Type = approvalType,
                    ApprovalMembers = new List<ApprovalMember>
                    {
                        new ApprovalMember
                        {
                            MemberId = User.Id
                        }
                    }
                };
                foreach (var member in approval.ApprovalMembers)
                {
                    member.Approval = approval;
                }
                cost.LatestCostStageRevision.Approvals = new List<Approval> { approval };

                EFContext.Approval.Add(approval);
                EFContext.UserUserGroup.Add(new UserUserGroup { UserId = User.Id });
                EFContext.Role.Add(new Role
                {
                    Id = approverRoleId,
                    Name = Roles.CostApprover
                });
                EFContext.SaveChanges();

                cost.Status = CostStageRevisionStatus.Approved;

                // Act
                var response = await ApprovalService.Approve(cost.Id, User, BuType.Pg);

                // Assert
                response.Should().NotBeNull();
                response.Success.Should().BeTrue();
            }
        }

        [TestFixture]
        public class RejectCostShould : ApprovalServiceTest
        {
            [Test]
            public async Task CallRejectOnApprovalServiceForLatestRevision()
            {
                // Arrange
                const ApprovalType approvalType = ApprovalType.IPM;
                var participantId = Guid.NewGuid();
                var approverRoleId = Guid.NewGuid();

                var cost = MockCost();

                var approval = new Approval
                {
                    CostStageRevisionId = cost.LatestCostStageRevision.Id,
                    Type = approvalType,
                    ApprovalMembers = new List<ApprovalMember>
                    {
                        new ApprovalMember
                        {
                            MemberId = User.Id
                        }
                    }
                };
                foreach (var member in approval.ApprovalMembers)
                {
                    member.Approval = approval;
                }
                cost.LatestCostStageRevision.Approvals = new List<Approval> { approval };

                EFContext.Approval.Add(approval);
                EFContext.UserUserGroup.Add(new UserUserGroup
                {
                    UserId = participantId,
                    Id = Guid.NewGuid(),
                    UserGroup = new UserGroup
                    {
                        ObjectId = cost.Id,
                        Id = Guid.NewGuid(),
                        RoleId = approverRoleId,
                    }
                });
                EFContext.Role.Add(new Role
                {
                    Id = approverRoleId,
                    Name = Roles.CostApprover
                });
                EFContext.SaveChanges();

                cost.Status = CostStageRevisionStatus.Approved;

                // Act
                var response = await ApprovalService.Reject(cost.Id, User, BuType.Pg, null);

                // Assert
                response.Should().NotBeNull();
                response.Success.Should().BeTrue();
                PermissionServiceMock.Verify(p => 
                    p.RevokeApproverAccess(cost.OwnerId, cost.Id, It.IsAny<IEnumerable<CostUser>>()),
                    Times.Once);

                PermissionServiceMock.Verify(p => 
                    p.GrantCostPermission(cost.Id, Roles.CostViewer, It.IsAny<IEnumerable<CostUser>>(), BuType.Pg, It.IsAny<Guid?>(), true),
                    Times.Once);
            }
        }
    }
}
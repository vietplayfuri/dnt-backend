using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using costs.net.core.Builders.Notifications;
using costs.net.core.ExternalResource.Paperpusher;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.core.Services.Notifications;
using costs.net.dataAccess;
using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Models.Stage;
using costs.net.plugins.PG.Services.Notifications;
using FluentAssertions;
using Moq;
using Brand = costs.net.dataAccess.Entity.Brand;
using Cost = costs.net.dataAccess.Entity.Cost;
using costs.net.tests.common.Stubs.EFContext;

namespace costs.net.plugins.tests.Services.Notifications
{
    [TestFixture]
    public class EmailNotificationServiceTests
    {
        private Mock<IEmailNotificationBuilder> _emailNotificationBuilderMock;
        private Mock<IPaperpusherClient> _paperPusherClientMock;
        private Mock<IEmailNotificationReminderService> _reminderServiceMock;
        private Mock<EFContext> _efContextMock;
        private Mock<ICostUserService> _costUserServiceMock;
        private Mock<IApprovalService> _approvalServiceMock;
        private EmailNotificationService _emailNotificationService;

        private Cost _cost;

        private readonly Guid _costId = Guid.NewGuid();
        private readonly Guid _costOwnerId = Guid.NewGuid();
        private readonly Guid _insuranceUserId = Guid.NewGuid();
        private readonly Guid _approverUserId = Guid.NewGuid();
        private readonly Guid _costStageRevisionId = Guid.NewGuid();

        private readonly string _insuranceUserGdamId = "dsaasdfakdsfyuiaweyrafw2134";

        [SetUp]
        public void Init()
        {
            _emailNotificationBuilderMock = new Mock<IEmailNotificationBuilder>();

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> approvedMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.BrandApprovalApproved, "57e5461ed9563f268ef4f19a")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildCostApprovedNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(approvedMessages));

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> pendingBrandMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.BrandApproverAssigned, "57e5461ed9563f268ef4f19a")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildPendingBrandApprovalNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(pendingBrandMessages));

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> pendingTechnicalMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.TechnicalApproverAssigned, "57e5461ed9563f268ef4f19a")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildPendingTechnicalApprovalNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(pendingTechnicalMessages));

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> recalledMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.Recalled, "57e5461ed9563f268ef4f19r")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildCostRecalledNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<CostUser>(), It.IsAny<DateTime>()))
                .ReturnsAsync(recalledMessages);

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> rejectedMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.Rejected, "57e5461ed9563f268ef4f19r")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildCostRejectedNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<CostUser>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(rejectedMessages));

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> cancelledMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.Cancelled, "57e5461ed9563f268ef4f19r")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildCostCancelledNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(cancelledMessages));

            IEnumerable<EmailNotificationMessage<CostNotificationObject>> costOwnerChangedMessages = new List<EmailNotificationMessage<CostNotificationObject>>
            {
                new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.CostOwnerChanged, "57e5461ed9563f268ef4f19r")
            };
            _emailNotificationBuilderMock.Setup(e => e.BuildCostOwnerChangedNotification(
                    It.IsAny<CostNotificationUsers>(),
                    It.IsAny<Cost>(),
                    It.IsAny<CostStageRevision>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CostUser>(),
                    It.IsAny<CostUser>()))
                .Returns(Task.FromResult(costOwnerChangedMessages));

            _paperPusherClientMock = new Mock<IPaperpusherClient>();
            _paperPusherClientMock.Setup(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()))
                .Returns(Task.FromResult(true));

            _reminderServiceMock = new Mock<IEmailNotificationReminderService>();
            _efContextMock = new Mock<EFContext>();
            _costUserServiceMock = new Mock<ICostUserService>();
            _approvalServiceMock = new Mock<IApprovalService>();
            SetupDataSharedAcrossTests();

            _emailNotificationService = new EmailNotificationService(_emailNotificationBuilderMock.Object, _paperPusherClientMock.Object, 
                _reminderServiceMock.Object, _efContextMock.Object, _costUserServiceMock.Object, _approvalServiceMock.Object);
        }

        #region CostHasBeenApproved Tests

        [Test]
        public async Task Fail_CostHasBeenApproved_Empty_CostId()
        {
            //Arrange
            var costId = Guid.Empty;
            var approverUserId = _approverUserId;
            var approvalType = ApprovalType.Brand.ToString();

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenApproved_Empty_UserId()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = Guid.Empty;
            var approvalType = ApprovalType.Brand.ToString();

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenApproved_Null_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            string approvalType = null;

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenApproved_Empty_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            var approvalType = string.Empty;

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenApproved_Invalid_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            var approvalType = "NotAnApprovalType";

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task CostHasBeenApproved_Brand_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            var approvalType = "Brand";

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
            _emailNotificationBuilderMock.Verify(e => e.BuildPendingBrandApprovalNotification(
                It.IsAny<CostNotificationUsers>(),
                It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task CostHasBeenApproved_IPM_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            var approvalType = "IPM";

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
            _emailNotificationBuilderMock.Verify(e => e.BuildPendingBrandApprovalNotification(
                It.IsAny<CostNotificationUsers>(),
                It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task CostHasBeenApproved_Technical_ApprovalType()
        {
            //Arrange
            var costId = _costId;
            var approverUserId = _approverUserId;
            var approvalType = "IPM";

            //Act
            bool result = await _emailNotificationService.CostHasBeenApproved(costId, approverUserId, approvalType);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
            _emailNotificationBuilderMock.Verify(e => e.BuildPendingBrandApprovalNotification(
                It.IsAny<CostNotificationUsers>(),
                It.IsAny<Cost>(), It.IsAny<CostStageRevision>(), It.IsAny<DateTime>()), Times.Once);
        }

        #endregion // CostHasBeenApproved Tests

        #region CostHasBeenRecalled Tests

        [Test]
        public async Task Fail_CostHasBeenRecalled_Empty_CostId()
        {
            //Arrange
            var costId = Guid.Empty;
            var costOwnerId = _costOwnerId;

            //Act
            bool result = await _emailNotificationService.CostHasBeenRecalled(costId, costOwnerId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenRecalled_Empty_UserId()
        {
            //Arrange
            var costId = _costId;
            var costOwnerId = Guid.Empty;

            //Act
            bool result = await _emailNotificationService.CostHasBeenRecalled(costId, costOwnerId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task CostHasBeenRecalled_ExpectedInput()
        {
            //Arrange
            var costId = _costId;
            var costOwnerId = _costOwnerId;

            //Act
            bool result = await _emailNotificationService.CostHasBeenRecalled(costId, costOwnerId);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        #endregion // CostHasBeenRecalled Tests

        #region CostHasBeenRejected Tests

        [Test]
        public async Task Fail_CostHasBeenRejected_Empty_CostId()
        {
            //Arrange
            var costId = Guid.Empty;
            var costOwnerId = _costOwnerId;
            var approvalType = ApprovalType.Brand.ToString();
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, costOwnerId, approvalType, comments);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenRejected_Empty_UserId()
        {
            //Arrange
            var costId = _costId;
            var userId = Guid.Empty;
            var approvalType = ApprovalType.Brand.ToString();
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, userId, approvalType, comments);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenRejected_Null_ApprovalType()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string approvalType = null;
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, userId, approvalType, comments);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenRejected_Empty_ApprovalType()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string approvalType = string.Empty;
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, userId, approvalType, comments);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenRejected_Invalid_ApprovalType()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string approvalType = "NotAnApprovalType";
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, userId, approvalType, comments);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task CostHasBeenRejected_Null_Comments()
        {
            //Arrange
            Guid costId = _costId;
            Guid approverId = _approverUserId;
            const string approvalType = "Brand";
            string comments = null; //It works if user does not have any comments

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, approverId, approvalType, comments);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task CostHasBeenRejected_Brand_ApprovalType()
        {
            //Arrange
            Guid costId = _costId;
            Guid userId = Guid.NewGuid();
            string approvalType = "Brand";
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, userId, approvalType, comments);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task CostHasBeenRejected_IPM_ApprovalType()
        {
            //Arrange
            Guid costId = _costId;
            Guid approverId = _approverUserId;
            string approvalType = "IPM";
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, approverId, approvalType, comments);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task CostHasBeenRejected_Technical_ApprovalType()
        {
            //Arrange
            Guid costId = _costId;
            Guid approverId = _approverUserId;
            string approvalType = "IPM";
            const string comments = "My comments";

            //Act
            bool result = await _emailNotificationService.CostHasBeenRejected(costId, approverId, approvalType, comments);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        #endregion // CostHasBeenRejected Tests

        #region CostHasBeenCancelled Tests

        [Test]
        public async Task Fail_CostHasBeenCancelled_Empty_CostId()
        {
            //Arrange
            Guid costId = Guid.Empty;

            //Act
            bool result = await _emailNotificationService.CostHasBeenCancelled(costId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostHasBeenCancelled_Cost_Status_NotCancelled()
        {
            //Arrange
            Guid costId = _costId;
            _cost.Status = CostStageRevisionStatus.PendingCancellation;

            //Act
            bool result = await _emailNotificationService.CostHasBeenCancelled(costId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }


        [Test]
        public async Task CostHasBeenCancelled_Success()
        {
            //Arrange
            Guid costId = _costId;
            _cost.Status = CostStageRevisionStatus.Cancelled;

            //Act
            bool result = await _emailNotificationService.CostHasBeenCancelled(costId);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        #endregion // CostHasBeenCancelled Tests

        #region CostOwnerChanged Tests

        [Test]
        public async Task Fail_CostOwnerChanged_Empty_CostId()
        {
            //Arrange
            Guid costId = Guid.Empty;
            Guid userId = Guid.NewGuid();
            Guid previousOwnerId = Guid.NewGuid();

            //Act
            bool result = await _emailNotificationService.CostOwnerHasChanged(costId, userId, previousOwnerId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostOwnerChanged_Empty_UserId()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            Guid userId = Guid.Empty;
            Guid previousOwnerId = Guid.NewGuid(); 

            //Act
            bool result = await _emailNotificationService.CostOwnerHasChanged(costId, userId, previousOwnerId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task Fail_CostOwnerChanged_Empty_PreviousOwnerId()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid previousOwnerId = Guid.Empty;
            //Act
            bool result = await _emailNotificationService.CostOwnerHasChanged(costId, userId, previousOwnerId);

            //Assert
            result.Should().BeFalse();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.Never);
        }

        [Test]
        public async Task CostOwnerChanged_Success()
        {
            //Arrange
            Guid costId = _costId;
            Guid userId = _costOwnerId;
            Guid previousOwnerId = _approverUserId;

            //Act
            bool result = await _emailNotificationService.CostOwnerHasChanged(costId, userId, previousOwnerId);

            //Assert
            result.Should().BeTrue();
            _paperPusherClientMock.Verify(p => p.SendMessage(It.IsAny<EmailNotificationMessage<CostNotificationObject>>()), Times.AtLeastOnce);
        }

        #endregion

        #region Private methods

        private void SetupDataSharedAcrossTests()
        {
            const string agencyLocation = "United Kingdom";
            const string agencyName = "Saatchi";
            const string brandName = "P&G";
            const string costNumber = "P101";
            const CostStages costStageName = CostStages.OriginalEstimate;
            const string costOwnerGdamUserId = "57e5461ed9563f268ef4f19d";
            const string costOwnerFullName = "Mr Cost Owner";
            const string projectName = "Pampers";
            const string projectGdamId = "57e5461ed9563f268ef4f19c";
            const string projectNumber = "PandG01";

            var projectId = Guid.NewGuid();

            _cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.FinanceManager,
                            Value = Constants.BusinessRole.FinanceManager
                        }
                    }
                }
            };
            var approverUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.Ipm,
                            Value = Constants.BusinessRole.Ipm
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new dataAccess.Entity.Project();
            var brand = new Brand();
            var agency = new dataAccess.Entity.Agency();
            var country = new Country();

            agency.Country = country;
            approverUser.Id = _approverUserId;
            _cost.CostNumber = costNumber;
            _cost.LatestCostStageRevision = latestRevision;
            _cost.Project = project;
            _cost.Owner = costOwner;
            costOwner.Agency = agency;
            costOwner.Id = _costOwnerId;
            insuranceUser.Id = _insuranceUserId;
            latestRevision.CostStage = costStage;
            project.Brand = brand;

            agency.Name = agencyName;
            brand.Name = brandName;
            _cost.Id = _costId;
            costStage.Name = costStageName.ToString();
            costOwner.FullName = costOwnerFullName;
            costOwner.GdamUserId = costOwnerGdamUserId;
            costOwner.Id = _costOwnerId;
            latestRevision.Id = _costStageRevisionId;
            project.Id = projectId;
            project.Name = projectName;
            project.GdamProjectId = projectGdamId;
            project.AdCostNumber = projectNumber;
            country.Name = agencyLocation;

            var agencies = new List<dataAccess.Entity.Agency> { agency };
            var brands = new List<Brand> { brand };
            var costs = new List<Cost> { _cost };
            var costStages = new List<CostStageRevision> { latestRevision };
            var costUsers = new List<CostUser> { approverUser, costOwner, insuranceUser };
            var countries = new List<Country> { country };
            var projects = new List<dataAccess.Entity.Project> { project };
            var insuranceUsers = new List<string> { _insuranceUserGdamId };

            _efContextMock.MockAsyncQueryable(agencies.AsQueryable(), c => c.Agency);
            _efContextMock.MockAsyncQueryable(brands.AsQueryable(), c => c.Brand);
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), c => c.Cost);
            _efContextMock.MockAsyncQueryable(costStages.AsQueryable(), c => c.CostStageRevision);
            _efContextMock.MockAsyncQueryable(costUsers.AsQueryable(), c => c.CostUser);
            _efContextMock.MockAsyncQueryable(countries.AsQueryable(), c => c.Country);
            _efContextMock.MockAsyncQueryable(projects.AsQueryable(), c => c.Project);

            _costUserServiceMock.Setup(cus => cus.GetInsuranceUsers(It.IsAny<dataAccess.Entity.Agency>())).Returns(Task.FromResult(insuranceUsers));
            _efContextMock.MockAsyncQueryable(new List<NotificationSubscriber>
            {
                new NotificationSubscriber
                {
                    CostId = _cost.Id,
                    CostUserId = _costOwnerId,
                    CostUser = costOwner
                }
            }.AsQueryable(), a => a.NotificationSubscriber);
        }
        #endregion // Private methods
    }
}

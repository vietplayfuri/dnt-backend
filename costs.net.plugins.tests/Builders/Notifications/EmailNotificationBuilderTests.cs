namespace costs.net.plugins.tests.Builders.Notifications
{
    using core.Builders.Response;
    using core.Models.Notifications;
    using dataAccess.Entity;
    using plugins.PG.Models.Stage;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using plugins.PG.Form.UsageBuyout;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;
    using Cost = dataAccess.Entity.Cost;
    using Project = dataAccess.Entity.Project;

    public class EmailNotificationBuilderTests : EmailNotificationBuilderTestBase
    {        
        [Test]
        public async Task Fail_CostSubmitted_Empty_Cost_Input()
        {
            //Arrange
            var costUsers = new CostNotificationUsers();
            Cost cost = null;
            var costStageRevision = new CostStageRevision();
            var timestamp = DateTime.UtcNow;

            //Act
            try
            {
                IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, costStageRevision, timestamp);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            //Assert
            Assert.Fail();
        }

        [Test]
        public async Task Fail_CostSubmitted_Empty_costUsers_Input()
        {
            //Arrange
            CostNotificationUsers costUsers = null;
            var cost = new Cost();
            var costStageRevision = new CostStageRevision();
            var timestamp = DateTime.UtcNow;

            //Act
            try
            {
                IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, costStageRevision, timestamp);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            //Assert
            Assert.Fail();
        }

        [Test]
        public async Task Fail_CostSubmitted_Empty_CostStageRevision_Input()
        {
            //Arrange
            var costUsers = new CostNotificationUsers();
            var cost = new Cost();
            CostStageRevision costStageRevision = null;
            var timestamp = DateTime.UtcNow;

            //Act
            try
            {
                IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, costStageRevision, timestamp);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            //Assert
            Assert.Fail();
        }

        [Test]
        public async Task Fail_CostSubmitted_Min_Timestamp_Input()
        {
            //Arrange
            var costUsers = new CostNotificationUsers();
            var cost = new Cost();
            var costStageRevision = new CostStageRevision();
            DateTime timestamp = DateTime.MinValue;

            //Act
            try
            {
                IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, costStageRevision, timestamp);
            }
            catch (ArgumentException)
            {
                return;
            }
            //Assert
            Assert.Fail();
        }

        [Test]
        public async Task Fail_CostSubmitted_Max_Timestamp_Input()
        {
            //Arrange
            var costUsers = new CostNotificationUsers();
            var cost = new Cost();
            var costStageRevision = new CostStageRevision();
            DateTime timestamp = DateTime.MaxValue;

            //Act
            try
            {
                IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, costStageRevision, timestamp);
            }
            catch (ArgumentException)
            {
                return;
            }
            //Assert
            Assert.Fail();
        }

        [Test]
        public async Task Cost_Submitted_Create_Notification_For_Cost_Owner()
        {
            //Arrange
            const string expectedActionType = "notifySubmitted";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingTechnicalApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
        }

        [Test]
        public async Task PendingTechnicalApproval_Technical_Approval_Assigned_Create_Notification_For_Technical_Approver()
        {
            //Arrange
            const string expectedActionType = "technicalApproverAssigned";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";
            const string technicalApproverType = "CC";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingTechnicalApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();
            var technicalApproverAsCostUser = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApprover.CostUser = technicalApproverAsCostUser;

            technicalApproval.Type = ApprovalType.IPM; //IPM is Technical
            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;

            var approvals = new List<Approval> { technicalApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingTechnicalApprovalNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> technicalApproverMessage = notifications[0];
            TestCommonMessageDetails(technicalApproverMessage, expectedActionType, technicalApproverGdamUserId);
            technicalApproverMessage.Object.Approver.Should().NotBeNull();
            technicalApproverMessage.Object.Approver.Name.Should().NotBeNull();
            technicalApproverMessage.Object.Approver.Type.Should().NotBeNull();
            technicalApproverMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            technicalApproverMessage.Object.Approver.Type.Should().Be(technicalApproverType);
        }

        [Test]
        public async Task PendingTechnicalApproval_Remind_Technical_Approver_ADC_2698()
        {
            //Arrange
            const string expectedActionType = "technicalApproverSendReminder";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingTechnicalApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision()
            {
                Status = CostStageRevisionStatus.PendingTechnicalApproval
            };
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();
            var technicalApproverAsCostUser = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApprover.CostUser = technicalApproverAsCostUser;

            technicalApproval.Type = ApprovalType.IPM; //IPM is Technical
            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;

            var approvals = new List<Approval> { technicalApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostReminderNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> technicalApproverMessage = notifications[0];
            TestCommonMessageDetails(technicalApproverMessage, expectedActionType, technicalApproverGdamUserId);
            technicalApproverMessage.Object.Approver.Should().NotBeNull();
            technicalApproverMessage.Object.Approver.Name.Should().NotBeNull();
            technicalApproverMessage.Object.Approver.Name.Should().Be(technicalApproverName);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task PendingTechnicalApproval_When_NA_Cyclone_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "technicalApproverAssigned";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingTechnicalApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new [] { "Cyclone" }};
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(new List<Approval>());

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildPendingTechnicalApprovalNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            var financeManagerMessage = notifications[0];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }

        [Test]
        public async Task PendingBrandApproval_Brand_Approval_Assigned_Create_Notification_For_Brand_Approver_NACyclone()
        {
            //Arrange
            const string expectedActionType = "brandApproverAssigned"; //Brand Approver
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ba";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingBrandApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            //North American Budget Region and Cyclone Agencies only.
            agency.Labels = new[] { "Cyclone" };

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            var approvals = new List<Approval> { brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);
            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            //Technical approval is skipped
            EmailNotificationMessage<CostNotificationObject> brandApproverMessage = notifications[0];
            TestCommonMessageDetails(brandApproverMessage, expectedActionType, brandApproverGdamUserId);
            brandApproverMessage.Object.Approver.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Name.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Type.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Name.Should().Be(brandApproverName);
            brandApproverMessage.Object.Approver.Type.Should().Be(brandApproverType);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task PendingBrandlApproval_When_NA_Cyclone_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "brandApproverAssigned";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.PendingBrandApproval;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new[] { "Cyclone" } };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock
                .Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true))
                .ReturnsAsync(new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalMember>()
                    }
                });

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            var financeManagerMessage = notifications[0];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }

        [Test]
        public async Task PendingBrandApproval_Create_Notification_For_Brand_Approver_For_NA_Cyclone_Agency()
        {
            //Arrange
            const string expectedActionType = "brandApproverAssigned";
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";
            const string brandApproverType = "Brand";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();
            var technicalApproverAsCostUser = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            //Cyclone Agencies only and Cost Budget Region is North American.
            agency.Labels = new[] { "Cyclone" };

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            //Technical Approval is required before Brand Approval is sent
            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApprover.CostUser = technicalApproverAsCostUser;

            technicalApproval.Type = ApprovalType.IPM;
            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;
            technicalApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { technicalApproval, brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> brandApproverMessage = notifications[0];
            brandApproverMessage.Should().NotBeNull();

            TestCommonMessageDetails(brandApproverMessage, expectedActionType, brandApproverGdamUserId);

            brandApproverMessage.Object.Approver.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Name.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Type.Should().NotBeNull();
            brandApproverMessage.Object.Approver.Name.Should().Be(brandApproverName);
            brandApproverMessage.Object.Approver.Type.Should().Be(brandApproverType);
        }

        [Test]
        public async Task PendingBrandApproval_Do_Not_Create_Notification_For_Brand_Approver_For_External_FakeUser()
        {
            //Arrange
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var externalBrandApprover = new ApprovalMember();

            var brandApproverAsCostUser = new CostUser { Email = ApprovalMemberModel.BrandApprovalUserEmail };

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            //North American Budget Region and Cyclone Agencies only.
            agency.Labels = new[] { "Cyclone" };

            brandApproval.ApprovalMembers = new List<ApprovalMember> { externalBrandApprover };
            externalBrandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            var approvals = new List<Approval> { brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(0);
        }

        [Test]
        public async Task PendingBrandApproval_Do_Not_Create_Notification_For_Brand_Approver_For_NA_Non_Cyclone_Agency()
        {
            //Arrange
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();

            //NA non-Cyclone Agencies should not create a notification for backup approver
            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage,
                brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            var approvals = new List<Approval> { brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(0);
        }

        [Test]
        public async Task PendingBrandApproval_Do_Not_Create_Notification_For_Brand_Approver_For_Non_NA_Non_Cyclone_Agency()
        {
            //Arrange
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();

            //China non-Cyclone Agencies should not create a notification for non-backup approver
            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage,
                brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.China);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            var approvals = new List<Approval> { brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(0);
        }

        [Test]
        public async Task PendingBrandApproval_Do_Not_Create_Notification_For_FM_For_NA_Cyclone_Agency_When_No_Brand_Approvals()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            //China non-Cyclone Agencies should not create a notification for non-backup approver
            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage,
                brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            agency.Labels = new[] { "Cyclone" };
            var approvals = new List<Approval>();
            
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            var result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(0);
        }

        [Test]
        public async Task Technical_Approval_Approved_Create_Notification_For_CostOwner()
        {
            //Arrange
            const string expectedActionType = "technicalApprovalApproved";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";
            const string technicalApproverType = "Technical";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, technicalApproverName, technicalApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(technicalApproverType);
        }

        [Test]
        public async Task Brand_Approval_Approved_Create_Notification_For_CostOwner()
        {
            //Arrange
            const string expectedActionType = "brandApprovalApproved";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(brandApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
        }

        [Test]
        public async Task Brand_Approval_Approved_When_NorthAmerica_And_OriginalEstimateCostStage_And_Cyclone_Should_Create_NotificationForFinanceManagement()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string financeManagementGdamUserId = "57e5461ed9563f268ef4f1fm";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            var costStageKey = CostStages.OriginalEstimate.ToString();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var financeManagementUserId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency
            {
                Labels = new[] { Constants.Agency.CycloneLabel }
            };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagementUsers =
            {
                financeManagementGdamUserId
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp)).ToArray();

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> financeManagementMessage = notifications[1];

            TestCommonMessageDetails(financeManagementMessage, expectedActionType, financeManagementGdamUserId);

            var financeManagementNotification = financeManagementMessage.Object as FinanceManagerCostNotificationObject;

            financeManagementMessage.Object.Approver.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Type.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().Be(brandApproverName);
            financeManagementMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);

            financeManagementNotification.Should().NotBeNull();
            financeManagementNotification.CanAssignIONumber.Should().BeTrue();
        }

        [Test]
        public async Task Brand_Approval_Approved_When_NorthAmerica_And_FirstPresentationCostStage_And_Cyclone_Should_Create_NotificationForFinanceManagement()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string financeManagementGdamUserId = "57e5461ed9563f268ef4f1fm";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            var costStageKey = CostStages.FirstPresentation.ToString();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var financeManagementUserId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency
            {
                Labels = new[] { Constants.Agency.CycloneLabel }
            };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagementUsers =
            {
                financeManagementGdamUserId
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp)).ToArray();

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> financeManagementMessage = notifications[1];

            TestCommonMessageDetails(financeManagementMessage, expectedActionType, financeManagementGdamUserId);

            var financeManagementNotification = financeManagementMessage.Object as FinanceManagerCostNotificationObject;

            financeManagementMessage.Object.Approver.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Type.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().Be(brandApproverName);
            financeManagementMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);

            financeManagementNotification.Should().NotBeNull();
            financeManagementNotification.CanAssignIONumber.Should().BeFalse();
        }
        [Test]
        public async Task Brand_Approval_Approved_When_NorthAmerica_And_OriginalEstimateCostStage_And_Cyclone_And_MultipleFinanceManagers_Should_Create_NotificationForEachFinanceManager()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string financeManagementGdamUserId1 = "57e5461ed9563f268ef4f1f1";
            const string financeManagementGdamUserId2 = "57e5461ed9563f268ef4f1f2";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            var costStageKey = CostStages.OriginalEstimate.ToString();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency
            {
                Labels = new[] { Constants.Agency.CycloneLabel }
            };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagements = {
                financeManagementGdamUserId1,
                financeManagementGdamUserId2
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagements,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp)).ToArray();

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var financeManagementMessage = notifications[1];

            TestCommonMessageDetails(financeManagementMessage, expectedActionType, financeManagementGdamUserId1, financeManagementGdamUserId2);

            financeManagementMessage.Object.Approver.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Type.Should().NotBeNull();
            financeManagementMessage.Object.Approver.Name.Should().Be(brandApproverName);
            financeManagementMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
            financeManagementMessage.Viewers.Count.Should().Be(2);
            financeManagementMessage.Recipients.Count.Should().Be(2);
        }

        [Test]
        public async Task Brand_Approval_Approved_When_NorthAmerica_And_OriginalEstimateCostStage_And_NotCyclone_Should_NotCreate_NotificationForFinanceManagement()
        {
            //Arrange
            const string financeManagementGdamUserId = "57e5461ed9563f268ef4f1fm";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            var costStageKey = CostStages.OriginalEstimate.ToString();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var financeManagementUserId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagementUsers =
            {
                financeManagementGdamUserId
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp)).ToArray();

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);
        }

        [Test]
        public async Task Brand_Approval_Approved_Do_Not_Create_Notification_For_FinanceManagement_NorthAmerican_NonOriginalEstimateCost()
        {
            //Arrange
            const string financeManagementGdamUserId = "57e5461ed9563f268ef4f1fm";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            var costStageKey = CostStages.Aipe.ToString(); //Not OriginalEstimate so not notification required.
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var financeManagementUserId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            
            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagementUsers =
            {
                financeManagementGdamUserId
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, core.Constants.EmailNotificationActionType.BrandApprovalApproved, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(brandApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
        }

        [Test]
        public async Task Brand_Approval_Approved_Do_Not_Create_Notification_For_FinanceManagement_NonNorthAmerican_OriginalEstimateCost()
        {
            //Arrange
            const string financeManagementGdamUserId = "57e5461ed9563f268ef4f1fm";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            var costStageKey = CostStages.OriginalEstimate.ToString();
            var budgetRegion = Constants.BudgetRegion.AsiaPacific;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var financeManagementUserId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId, budgetRegion);

            latestRevision.CostStage.Key = costStageKey;

            string[] financeManagementUsers =
            {
                financeManagementGdamUserId
            };

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, core.Constants.EmailNotificationActionType.BrandApprovalApproved, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(brandApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task Approved_When_NA_Cyclone_Any_Stage_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.Approved;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";
            const string approverName = "approver user 1";
            var approvalType = ApprovalType.Brand.ToString();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new[] { "Cyclone" } };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(new List<Approval>());

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId },
                Watchers = new List<string>()
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost, latestRevision, approverName, approvalType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2); // Cost Owner and Finance Manager

            var financeManagerMessage = notifications[1];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }
        
        [Test]
        public async Task Cost_Recalled_Create_Notification_For_Cost_Owner()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var recaller = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            recaller.Id = userId;
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers,
                cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
        }

        [Test]
        public async Task Cost_Recalled_Create_Notification_For_Technical_Approver()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var recaller = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var technicalApproverAsCostUser = new CostUser();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApproval.Type = ApprovalType.IPM;

            technicalApprover.CostUser = technicalApproverAsCostUser;

            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;
            technicalApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { technicalApproval };
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers,
                cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            EmailNotificationMessage<CostNotificationObject> technicalApproverMessage = notifications[1];
            TestCommonMessageDetails(technicalApproverMessage, expectedActionType, technicalApproverGdamUserId);
        }

        [Test]
        public async Task Cost_Recalled_Create_Notification_For_Brand_Approver()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ba";
            const string brandApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var recaller = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproverAsCostUser = new CostUser();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApproval.Type = ApprovalType.Brand;

            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;
            brandApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { brandApproval };
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);


            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            EmailNotificationMessage<CostNotificationObject> brandApproverMessage = notifications[1];
            TestCommonMessageDetails(brandApproverMessage, expectedActionType, brandApproverGdamUserId);
        }

        [Test]
        public async Task Cost_Recalled_Do_Not_Create_Notification_For_External_Approver()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ba";
            const string brandApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproverAsCostUser = new CostUser { Email = ApprovalMemberModel.BrandApprovalUserEmail };
            var brandApproval = new Approval();
            var coupaBrandApprover = new ApprovalMember();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { coupaBrandApprover };
            brandApproval.Type = ApprovalType.Brand;

            coupaBrandApprover.CostUser = brandApproverAsCostUser;

            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;
            brandApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { brandApproval };
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var recaller = new CostUser();
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];
            //Cost owner notification only
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
        }

        [Test]
        public async Task Cost_Recalled_Create_Notification_For_Many_Approvers()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";

            var costId = Guid.NewGuid();
            var brandApproverUserId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproverAsCostUser = new CostUser();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var technicalApproverAsCostUser = new CostUser();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApproval.Type = ApprovalType.Brand;

            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;
            brandApproverAsCostUser.Id = brandApproverUserId;

            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApproval.Type = ApprovalType.IPM;

            technicalApprover.CostUser = technicalApproverAsCostUser;

            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;
            technicalApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { brandApproval, technicalApproval };
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var recaller = new CostUser();

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(3);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            EmailNotificationMessage<CostNotificationObject> technicalApproverMessage = notifications[1];
            TestCommonMessageDetails(technicalApproverMessage, expectedActionType, technicalApproverGdamUserId);

            EmailNotificationMessage<CostNotificationObject> brandApproverMessage = notifications[2];
            TestCommonMessageDetails(brandApproverMessage, expectedActionType, brandApproverGdamUserId);
        }

        [Test]
        public async Task Cost_Recalled_Create_Notification_MessageDetails()
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const string requisition = "Test Requisition";
            const string airingCountryName = "Airing country name";
            const string region = "Region";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, budgetRegion: region);

            CustomObjectDataServiceMock.Setup(cod => cod.GetCustomData<PgPurchaseOrderResponse>(costStageRevisionId, CustomObjectDataKeys.PgPurchaseOrderResponse))
                .ReturnsAsync(new PgPurchaseOrderResponse
                {
                    Requisition = requisition
                });

            CostFormServiceMock.Setup(cf => cf.GetCostFormDetails<BuyoutDetails>(costStageRevisionId)).ReturnsAsync(new BuyoutDetails
            {
                AiringCountries = new[]
                {
                    new BuyoutDetails.Country
                    {
                        Name = airingCountryName
                    }
                }
            });

            var recaller = new CostUser();

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp))?.ToArray();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var costOwnerMessage = result[0];

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Cost.Requisition.Should().Be(requisition);
            costOwnerMessage.Object.Cost.AiringCountries.Should().Be(airingCountryName);
            costOwnerMessage.Object.Cost.Region.Should().Be(region);
        }

        [Test]
        public async Task Cost_Recalled_Create_Notification_For_Requisitioner()
        {
            //Arrange
            const string requisitionerGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string requisitionerName = "John Smith";

            var costId = Guid.NewGuid();
            var requisitionerUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval
            {
                ApprovalMembers = new List<ApprovalMember>(),
                Requisitioners = new List<Requisitioner>
                {
                    new Requisitioner
                    {
                        CostUser = new CostUser
                        {
                            Id = requisitionerUserId,
                            GdamUserId = requisitionerGdamUserId,
                            FullName = requisitionerName
                        }
                    }
                },
                Type = ApprovalType.Brand
            };
            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            var approvals = new List<Approval> { brandApproval };
            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var recaller = new CostUser();

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            //Act
            var result = (await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp))?.ToArray();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var costOwnerMessage = result[0];
            var requisitionerMessage = result[1];
            requisitionerMessage.Recipients.ToArray()[0].Should().Be(requisitionerGdamUserId);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task Cost_Recalled_When_NA_Cyclone_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "notifyRecalled";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.Recalled;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";
            var recaller = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new[] { "Cyclone" } };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(new List<Approval>());

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId },
                Watchers = new List<string>()
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostRecalledNotification(costUsers, cost, latestRevision, recaller, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2); // Cost Owner and Finance Manager

            var financeManagerMessage = notifications[1];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }

        [Test]
        public async Task Cost_Rejected_Create_Notification_For_Cost_Owner()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string costOwnerParent = "costowner";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            costOwnerMessage.Object.Comments.Should().NotBeNull();
            costOwnerMessage.Object.Comments.Should().Be(comments);

            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task Cost_Rejected_When_NA_Cyclone_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.Rejected;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";
            var rejecter = new CostUser();
            var approvalType = ApprovalType.IPM.ToString();
            var rejectionComments = "Rejected because I'm lazy";

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new[] { "Cyclone" } };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(new List<Approval>());

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId },
                Watchers = new List<string>()
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(
                costUsers, cost, latestRevision, rejecter, approvalType, rejectionComments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2); // Cost Owner and Finance Manager

            var financeManagerMessage = notifications[1];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }

        [Test]
        public async Task Cost_Cancelled_Create_Notification_For_Cost_Owner()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string costOwnerParent = "costowner";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);
        }

        [Test]
        [TestCase(CostStages.OriginalEstimate)]
        [TestCase(CostStages.OriginalEstimateRevision)]
        [TestCase(CostStages.FirstPresentation)]
        [TestCase(CostStages.FirstPresentationRevision)]
        [TestCase(CostStages.FinalActual)]
        [TestCase(CostStages.FinalActualRevision)]
        public async Task Cost_Cancelled_When_NA_Cyclone_Notification_For_FinanceManager(CostStages stage)
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const CostStageRevisionStatus costStageRevisionStatus = CostStageRevisionStatus.Cancelled;

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            const string financeManagerGdamId = "234982382390";

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage { Key = stage.ToString() };
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency { Labels = new[] { "Cyclone" } };
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner,
                costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(new List<Approval>());

            cost.Status = costStageRevisionStatus;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = new List<string> { financeManagerGdamId },
                Watchers = new List<string>()
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(
                costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2); // Cost Owner and Finance Manager

            var financeManagerMessage = notifications[1];
            TestCommonMessageDetails(financeManagerMessage, expectedActionType, financeManagerGdamId);
        }

        [Test]
        public async Task Approver_Removed_Create_Notification_For_Previous_Approver()
        {
            //Arrange
            const string expectedActionType = core.Constants.EmailNotificationActionType.ApproverUnassigned;
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ba";
            const string brandApproverName = "John Smith";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "John Smith";

            var costId = Guid.NewGuid();
            var technicalApproverUserId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();

            var previousRevision = new CostStageRevision();
            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();
            var technicalApproval = new Approval();
            var technicalApprover = new ApprovalMember();
            var technicalApproverAsCostUser = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, previousRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId, Constants.BudgetRegion.NorthAmerica);

            //Cyclone Agencies only and Cost Budget Region is North American.
            agency.Labels = new[] { "Cyclone" };

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;
            brandApprover.Approval = brandApproval;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            //Technical Approval is required before Brand Approval is sent
            technicalApproval.ApprovalMembers = new List<ApprovalMember> { technicalApprover };
            technicalApprover.CostUser = technicalApproverAsCostUser;
            technicalApprover.Approval = technicalApproval;

            technicalApproval.Type = ApprovalType.IPM;
            technicalApproverAsCostUser.GdamUserId = technicalApproverGdamUserId;
            technicalApproverAsCostUser.FullName = technicalApproverName;
            technicalApproverAsCostUser.Id = technicalApproverUserId;

            var approvals = new List<Approval> { technicalApproval, brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            var removedApprovalMembers = new List<ApprovalMember>
            {
                brandApprover,
                technicalApprover,
            };
            latestRevision.Approvals = new List<Approval>();
            cost.LatestCostStageRevision = latestRevision;

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPreviousApproverNotification(costUsers,
                removedApprovalMembers,
                cost, previousRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> brandApproverMessage = notifications[0];
            TestCommonMessageDetails(brandApproverMessage, expectedActionType, brandApproverGdamUserId);
            EmailNotificationMessage<CostNotificationObject> technicalApproverMessage = notifications[1];
            TestCommonMessageDetails(technicalApproverMessage, expectedActionType, technicalApproverGdamUserId);
        }
    }
}

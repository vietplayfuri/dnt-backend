
namespace costs.net.plugins.tests.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Notifications;
    using core.Models.Payments;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Models;
    using plugins.PG.Models.Stage;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;
    using Cost = dataAccess.Entity.Cost;
    using Project = dataAccess.Entity.Project;

    [TestFixture]
    public class CoupaNotificationTests : EmailNotificationBuilderTestBase
    {
        private const string PoNumber = "A PO Number";

        [Test]
        public async Task Create_Notification_For_Coupa_For_Non_NA_Non_Cyclone_Agency_When_Cost_Total_Amount_Increased()
        {
            //Arrange
            decimal? previousPaymentAmount = 10.00M;
            decimal? latestPaymentAmount = 15.00M; //Increased from 10
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var latestRevision = new CostStageRevision();

            var previousRevisionId = Guid.NewGuid();
            var latestRevisionId = Guid.NewGuid();

            SetupPurchaseOrderCost(cost, latestRevision, costOwner, previousRevisionId, latestRevisionId, PoNumber);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            var previousPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = previousPaymentAmount
            };
            var latestPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = latestPaymentAmount
            };
            
            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(previousRevisionId, false)).Returns(Task.FromResult(previousPaymentResult));
            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(latestRevisionId, false)).Returns(Task.FromResult(latestPaymentResult));

            //Act
            IEnumerable <EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);
        }

        [Test]
        public async Task Create_Notification_For_Coupa_For_Non_NA_Non_Cyclone_Agency_When_Cost_Total_Amount_Decreased()
        {
            //Arrange
            decimal? previousPaymentAmount = 10.00M;
            decimal? latestPaymentAmount = 5.00M; //Decreased from 10
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var latestRevision = new CostStageRevision();

            var previousRevisionId = Guid.NewGuid();
            var latestRevisionId = Guid.NewGuid();

            SetupPurchaseOrderCost(cost, latestRevision, costOwner, previousRevisionId, latestRevisionId, PoNumber);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            var previousPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = previousPaymentAmount
            };
            var latestPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = latestPaymentAmount
            };

            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(previousRevisionId, false)).Returns(Task.FromResult(previousPaymentResult));
            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(latestRevisionId, false)).Returns(Task.FromResult(latestPaymentResult));

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);
        }

        [Test]
        public async Task Do_Not_Create_Notification_For_Coupa_For_Non_NA_Non_Cyclone_Agency_When_Cost_Total_Amount_Is_Unchanged()
        {
            //Arrange
            decimal? previousPaymentAmount = 10.00M;
            decimal? latestPaymentAmount = 10.00M; //Unchanged from 10
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser();
            var latestRevision = new CostStageRevision();

            var previousRevisionId = Guid.NewGuid();
            var latestRevisionId = Guid.NewGuid();

            SetupPurchaseOrderCost(cost, latestRevision, costOwner, previousRevisionId, latestRevisionId, PoNumber);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner
            };

            var previousPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = previousPaymentAmount
            };
            var latestPaymentResult = new PaymentAmountResult
            {
                TotalCostAmount = latestPaymentAmount
            };

            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(previousRevisionId, false)).Returns(Task.FromResult(previousPaymentResult));
            PgPaymentServiceMock.Setup(ppsm => ppsm.GetPaymentAmount(latestRevisionId, false)).Returns(Task.FromResult(latestPaymentResult));

            //Act
            IEnumerable<EmailNotificationMessage<CostNotificationObject>> result = await EmailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost,
                latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(0);
        }

        private void SetupPurchaseOrderCost(Cost cost, CostStageRevision latestRevision, 
            CostUser costOwner, Guid previousRevisionId, Guid latestRevisionId, string poNumber)
        {
            const string brandApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string brandApproverName = "John Smith";
            var costId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            
            var previousStageId = Guid.NewGuid();
            var latestStageId = Guid.NewGuid();

            var previousRevision = new CostStageRevision();
            var previousStage = new CostStage();            
            var latestStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var brandApproval = new Approval();
            var brandApprover = new ApprovalMember();
            var brandApproverAsCostUser = new CostUser();

            previousRevision.CostStage = previousStage;
            previousRevision.Id = previousRevisionId;

            previousStage.Id = previousRevision.CostStageId = previousStageId;
            latestStage.Id = latestRevision.CostStageId = latestStageId;

            previousStage.Name = CostStages.OriginalEstimate.ToString();
            latestStage.Name = CostStages.FinalActual.ToString();

            cost.CostStages.AddRange(new[] { previousStage, latestStage });

            //China non-Cyclone Agencies should create a notification for non-backup approver when the cost total amount has changed
            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, latestStage,
                brand, costId, latestRevisionId, projectId, Constants.BudgetRegion.China);

            brandApproval.ApprovalMembers = new List<ApprovalMember> { brandApprover };
            brandApprover.CostUser = brandApproverAsCostUser;

            brandApproval.Type = ApprovalType.Brand;
            brandApproverAsCostUser.GdamUserId = brandApproverGdamUserId;
            brandApproverAsCostUser.FullName = brandApproverName;

            var approvals = new List<Approval> { brandApproval };

            ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), true)).ReturnsAsync(approvals);

            var pgPaymentDetails = new PgPaymentDetails
            {
                PoNumber = poNumber
            };
            CostStageServiceMock.Setup(cssm => cssm.GetPreviousCostStage(latestStageId)).Returns(Task.FromResult(previousStage));
            CostStageRevisionServiceMock.Setup(csrsm => csrsm.GetLatestRevision(previousStageId)).Returns(Task.FromResult(previousRevision));
            CustomObjectDataServiceMock
                .Setup(codsm => codsm.GetCustomData<PgPaymentDetails>(latestRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .Returns(Task.FromResult(pgPaymentDetails));
            CustomObjectDataServiceMock
                .Setup(codsm => codsm.GetCustomData<PgPaymentDetails>(previousRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .Returns(Task.FromResult(pgPaymentDetails));
        }
    }
}

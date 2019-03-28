namespace costs.net.plugins.tests.PG.Services.PurchaseOrder
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.Response;
    using core.Messaging.Messages;
    using core.Models;
    using core.Models.Payments;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.CustomData;
    using core.Services.Events;
    using core.Services.Notifications;
    using core.Services.Workflow;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Services;
    using plugins.PG.Services.PurchaseOrder;

    [TestFixture]
    public class PgPurchaseOrderResponseConsumerTests
    {
        private EFContext _efContext;
        private Mock<ICustomObjectDataService> _customDataServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IApprovalService> _approvalServiceMock;
        private Mock<IEmailNotificationService> _emailNotificationServiceMock;
        private Mock<ICostActionService> _costActionServiceMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IPgPaymentService> _pgPaymentServiceMock;
        private Mock<IActivityLogService> _activityLogServiceMock;
        private Guid _costId;
        private Guid _brandApproverId;
        private const string CostNumber = "test cost number";
        private Cost _cost;

        private PgPurchaseOrderResponseConsumer _consumer;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _customDataServiceMock = new Mock<ICustomObjectDataService>();
            _mapperMock = new Mock<IMapper>();
            _approvalServiceMock = new Mock<IApprovalService>();
            _emailNotificationServiceMock = new Mock<IEmailNotificationService>();
            _costActionServiceMock = new Mock<ICostActionService>();
            _eventServiceMock = new Mock<IEventService>();
            _pgPaymentServiceMock = new Mock<IPgPaymentService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            _costId = Guid.NewGuid();
            _brandApproverId = Guid.NewGuid();

            _cost = new Cost
            {
                Id = _costId,
                CostNumber = CostNumber,
                LatestCostStageRevisionId = Guid.NewGuid()
            };
            var brandApprover = new CostUser
            {
                Id = _brandApproverId,
                Email = ApprovalMemberModel.BrandApprovalUserEmail
            };

            _efContext.Cost.Add(_cost);
            _efContext.CostUser.Add(brandApprover);
            _efContext.SaveChanges();
            _consumer = new PgPurchaseOrderResponseConsumer(
                _efContext,
                _customDataServiceMock.Object,
                _mapperMock.Object,
                _approvalServiceMock.Object,
                _emailNotificationServiceMock.Object,
                _costActionServiceMock.Object,
                _eventServiceMock.Object,
                _pgPaymentServiceMock.Object,
                _activityLogServiceMock.Object
                );
        }

        [Test]
        [TestCase(null)]
        [TestCase("InvalidData")]
        public async Task Consume_Send_Comments_To_ApprovalService(string comments)
        {
            // Arrange
            const string activityType = ActivityTypes.Updated;
            const string approvalStatus = ApprovalStatuses.Rejected;

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    Comments = comments,
                    ApprovalStatus = approvalStatus
                }))
            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };

            _approvalServiceMock.Setup(a => a.Reject(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, comments, It.IsAny<SourceSystem>())).Returns(Task.FromResult(operationResponse));
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), string.Empty)).Returns(Task.FromResult(true));

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            //Assert
            _approvalServiceMock.Verify(s => s.Reject(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, comments, It.IsAny<SourceSystem>()));
            _emailNotificationServiceMock.Verify(s => s.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), ApprovalType.Brand.ToString(), comments));
        }

        [Test]
        public async Task Consume_whenUpdatedAndStatusApprovedAndPendingApproval_shouldApproveTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.Approved;
            var requisition = "requisitionId";

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus
                }))
            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            var dbPurchaseOrderResponse = new PgPurchaseOrderResponse()
            {
                Requisition = requisition,
            };
            var dbPaymentDetails = new PgPaymentDetails()
            {
                Requisition = requisition,
            };

            var cost = _efContext.Cost.Find(_costId);
            cost.Status = CostStageRevisionStatus.PendingBrandApproval;
            _efContext.Cost.Update(cost);
            await _efContext.SaveChangesAsync();

            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPurchaseOrderResponse));
            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPaymentDetails>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPaymentDetails));
            _customDataServiceMock.Setup(cds => cds.Save(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<UserIdentity>())).Returns(Task.FromResult(new CustomObjectData()));
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).Returns(Task.FromResult(operationResponse));
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa), Times.Once);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(requisition, dbPurchaseOrderResponse.Requisition);
            Assert.AreEqual(requisition, dbPaymentDetails.Requisition);
        }

        [Test]
        public async Task Consume_whenUpdatedAndStatusApprovedAndNotPendingApproval_shouldNotApproveTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.Approved;
            var requisition = "requisitionId";

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus
                }))
            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            var dbPurchaseOrderResponse = new PgPurchaseOrderResponse()
            {
                Requisition = requisition,
            };
            var dbPaymentDetails = new PgPaymentDetails()
            {
                Requisition = requisition,
            };
            _cost.Status = CostStageRevisionStatus.Approved;
            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPurchaseOrderResponse));
            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPaymentDetails>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPaymentDetails));
            _customDataServiceMock.Setup(cds => cds.Save(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<UserIdentity>())).Returns(Task.FromResult(new CustomObjectData()));
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).Returns(Task.FromResult(operationResponse));
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa), Times.Never);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(requisition, dbPurchaseOrderResponse.Requisition);
            Assert.AreEqual(requisition, dbPaymentDetails.Requisition);
        }

        [Test]
        public async Task Consume_whenUpdatedAndStatusRejected_shouldRejectTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.Rejected;

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus
                }))

            };
            var operationResponse = new ApprovalServiceActionResult()
            {
                Success = true
            };
            var dbPurchaseOrderResponse = new PgPurchaseOrderResponse()
            {
                Requisition = "requisitionId",
            };
            var dbPaymentDetails = new PgPaymentDetails()
            {
                Requisition = "requisitionId",
            };
            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPurchaseOrderResponse));
            _customDataServiceMock.Setup(cds => cds.GetCustomData<PgPaymentDetails>(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(dbPaymentDetails));
            _customDataServiceMock.Setup(cds => cds.Save(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<UserIdentity>())).Returns(Task.FromResult(new CustomObjectData()));
            _approvalServiceMock.Setup(a => a.Reject(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, It.IsAny<string>(), It.IsAny<SourceSystem>())).Returns(Task.FromResult(operationResponse));
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Reject(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, It.IsAny<string>(), It.IsAny<SourceSystem>()), Times.Once);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _customDataServiceMock.Verify(cds => cds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _customDataServiceMock.Verify(cds => cds.GetCustomData<PgPaymentDetails>(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            Assert.IsNull(dbPurchaseOrderResponse.Requisition);
            Assert.IsNull(dbPaymentDetails.Requisition);
        }

        [Test]
        public async Task Consume_whenUpdated_And_StatusAwaitingDecisionInCost_And_TotalAmountTheSameAsCurrent_And_CostStatusIsPendingBrandApproval_shouldApproveTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.AwaitingDecisionInCost;
            const decimal totalAmountIncoming = (decimal)10000.23; // 2 Digits precision defined on schema level
            const decimal totalAmountCurrent = (decimal)10000.226; // Rounded to 2 digits amount is still the same as totalAmountIncoming
            //_cost.Status = CostStageRevisionStatus.PendingBrandApproval;
            var cost = _efContext.Cost.Find(_costId);
            cost.Status = CostStageRevisionStatus.PendingBrandApproval;
            _efContext.Cost.Update(cost);
            await _efContext.SaveChangesAsync();

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus,
                    TotalAmount = totalAmountIncoming
                }))

            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).ReturnsAsync(operationResponse);
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _pgPaymentServiceMock.Setup(p => p.GetPaymentAmount(It.Is<Guid>(id => id == _cost.LatestCostStageRevisionId.Value), false))
                .ReturnsAsync(new PaymentAmountResult
                {
                    TotalCostAmount = totalAmountCurrent
                });

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa), Times.Once);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Consume_whenUpdated_And_StatusAwaitingDecisionInCost_And_TotalAmountIsMoreThanCurrent_shouldNotApproveTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.AwaitingDecisionInCost;
            const decimal totalAmountIncoming = (decimal)10000.23; // 2 Digits precision defined on schema level
            const decimal totalAmountCurrent = (decimal)10000.224; // Rounded to 2 digits amount is 10000.22 which less than totalAmountIncoming 

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus,
                    TotalAmount = totalAmountIncoming
                }))

            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).ReturnsAsync(operationResponse);
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _pgPaymentServiceMock.Setup(p => p.GetPaymentAmount(It.Is<Guid>(id => id == _cost.LatestCostStageRevisionId.Value), false))
                .ReturnsAsync(new PaymentAmountResult
                {
                    TotalCostAmount = totalAmountCurrent
                });

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa), Times.Never);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Consume_whenUpdated_And_StatusAwaitingDecisionInCost_And_TotalAmountIsLessThanCurrent_shouldNotApproveTheCost()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;
            var approvalStatus = ApprovalStatuses.AwaitingDecisionInCost;
            const decimal totalAmountIncoming = (decimal)10000.224; // 2 Digits precision defined on schema level
            const decimal totalAmountCurrent = (decimal)10000.23; // Rounded to 2 digits amount is 10000.22 which less than totalAmountIncoming 

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    ApprovalStatus = approvalStatus,
                    TotalAmount = totalAmountIncoming
                }))

            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).ReturnsAsync(operationResponse);
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenRejected(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _pgPaymentServiceMock.Setup(p => p.GetPaymentAmount(It.Is<Guid>(id => id == _cost.LatestCostStageRevisionId.Value), false))
                .ReturnsAsync(new PaymentAmountResult
                {
                    TotalCostAmount = totalAmountCurrent
                });

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _approvalServiceMock.Verify(s => s.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa), Times.Never);
            _emailNotificationServiceMock.Verify(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Consume_Always_Should_SavePurchaseOrderResponse_AndUpdatePaymentDetails()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;

            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = CostNumber,
                ClientName = BuType.Pg.ToString(),
                Payload = JObject.Parse(JsonConvert.SerializeObject(new PgPurchaseOrderResponse()))
            };
            var operationResponse = new ApprovalServiceActionResult
            {
                Success = true
            };
            _approvalServiceMock.Setup(a => a.Approve(It.IsAny<Guid>(), It.IsAny<UserIdentity>(), BuType.Pg, SourceSystem.Coupa)).Returns(Task.FromResult(operationResponse));
            _emailNotificationServiceMock.Setup(em => em.CostHasBeenApproved(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // Act
            await _consumer.Consume(purchaseOrderResponse);

            // Assert
            _customDataServiceMock.Verify(ds => ds.Save(It.IsAny<Guid>(), It.Is<string>(s => s == CustomObjectDataKeys.PgPurchaseOrderResponse), It.IsAny<object>(), It.IsAny<UserIdentity>()));
            _customDataServiceMock.Verify(ds => ds.Save(It.IsAny<Guid>(), It.Is<string>(s => s == CustomObjectDataKeys.PgPaymentDetails), It.IsAny<object>(), It.IsAny<UserIdentity>()));
        }

    }
}

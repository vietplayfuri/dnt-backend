namespace costs.net.messaging.test.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Response;
    using core.Messaging.Messages;
    using core.Models;
    using core.Models.User;
    using core.Services.Costs;
    using core.Services.Notifications;
    using dataAccess;
    using dataAccess.Entity;
    using messaging.Handlers;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Serilog;
    using tests.common.Stubs.EFContext;
    using Cost = dataAccess.Entity.Cost;

    [TestFixture]
    public class PurchaseOrderErrorResponseHandlerTest
    {
        private Mock<EFContext> _efContextMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IApprovalService> _approvalServiceMock;
        private Mock<IEmailNotificationService> _emailNotificationService;
        private Mock<ISupportNotificationService> _supportNotificationService;

        private PurchaseOrderErrorResponseHandler _handler;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _loggerMock = new Mock<ILogger>();
            _approvalServiceMock = new Mock<IApprovalService>();
            _supportNotificationService = new Mock<ISupportNotificationService>();
            _emailNotificationService = new Mock<IEmailNotificationService>();

            _handler = new PurchaseOrderErrorResponseHandler(
                _efContextMock.Object,
                _approvalServiceMock.Object,
                _loggerMock.Object,
                _emailNotificationService.Object,
                _supportNotificationService.Object
            );
        }
        private Cost MockCost()
        {
            var costId = Guid.NewGuid();

            var costNumber = "AC" + (long) (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var createdById = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                CostNumber = costNumber,
                CreatedById = createdById,
                OwnerId = createdById
            };
            _efContextMock.MockAsyncQueryable(new List<Cost> { cost }.AsQueryable(), c => c.Cost);

            return cost;
        }

        [Test]
        [TestCase(ResponseErrorType.AwaitingActionInCosts)]
        public async Task Handle_ErrorMessageFromXmg_ShouldRejectCost(ResponseErrorType errorType)
        {
            // Arrange
            var cost = MockCost();

            var payload = new { errorMessages = new[] { new { type = ((int) errorType).ToString(), message = "Error messages" } } };

            var message = new PurchaseOrderErrorResponse
            {
                ActivityType = "Error",
                ClientName = BuType.Pg.ToString(),
                EventTimeStamp = DateTime.Now,
                CostNumber = cost.CostNumber,
                Payload = JObject.Parse(JsonConvert.SerializeObject(payload))
            };
            var response = new ApprovalServiceActionResult { Success = true, ApprovalType = "Brand" };

            var costUser = new CostUser { GdamUserId = "alsjdnaljsdn" };
            var adminUser = new CostUser { Email = ApprovalMemberModel.BrandApprovalUserEmail };
            var adminUserIdentity = new SystemAdminUserIdentity(adminUser);

            var costUserSetMock = _efContextMock.MockAsyncQueryable(new List<CostUser> { costUser, adminUser }.AsQueryable(), context => context.CostUser);
            costUserSetMock.Setup(u => u.FindAsync(It.IsAny<Guid>())).ReturnsAsync(costUser);

            _approvalServiceMock.Setup(a => a.Reject(cost.Id, adminUserIdentity, BuType.Pg, "Error messages", SourceSystem.Coupa)).ReturnsAsync(response);

            // Act
            await _handler.Handle(message);

            // Assert
            _approvalServiceMock.Verify(s => s.Reject(cost.Id, adminUserIdentity, BuType.Pg, "Error messages", SourceSystem.Coupa));
        }

        [Test]
        public async Task Handle_WhenTechnicalError_ShouldSendEmailToSupportTeam()
        {
            // Arrange
            var costNumber = "12345";

            var payload = new { errorMessages = new[] { new { type = ((int) ResponseErrorType.Technical).ToString(), message = "Error messages" } } };

            var message = new PurchaseOrderErrorResponse
            {
                ActivityType = "Error",
                ClientName = BuType.Pg.ToString(),
                EventTimeStamp = DateTime.Now,
                CostNumber = costNumber,
                Payload = JObject.Parse(JsonConvert.SerializeObject(payload))
            };

            // Act
            await _handler.Handle(message);

            // Assert
            _supportNotificationService.Verify(ens => ens.SendSupportErrorNotification(It.IsAny<string>(), It.IsAny<string>(), null),
                Times.Once);
        }
    }
}
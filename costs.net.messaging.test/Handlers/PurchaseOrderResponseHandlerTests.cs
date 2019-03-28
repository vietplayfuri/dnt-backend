namespace costs.net.messaging.test.Handlers
{
    using System;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Messaging.Messages;
    using core.Models;
    using core.Services.Notifications;
    using core.Services.PurchaseOrder;
    using FluentAssertions;
    using messaging.Handlers;
    using Serilog;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PurchaseOrderResponseHandlerTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<IPurchaseOrderResponseConsumer> _consumerMock;
        private Mock<ISupportNotificationService> _supportNotificationServiceMock;

        private PurchaseOrderResponseHandler _handler;

        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger>();
            _consumerMock = new Mock<IPurchaseOrderResponseConsumer>();
            _supportNotificationServiceMock = new Mock<ISupportNotificationService>();

            _handler = new PurchaseOrderResponseHandler(_loggerMock.Object, 
                new []
                {
                    new Lazy<IPurchaseOrderResponseConsumer, PluginMetadata>(
                        () => _consumerMock.Object, new PluginMetadata { BuType =  BuType.Pg })
                },
                _supportNotificationServiceMock.Object
                );
        }

        [Test]
        public async Task Handle_whenException_ShouldSendEmailToSupport()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;

            var costNumber = "test cost number";
            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = costNumber,
                ClientName = "Invalid"
            };

            // Act
            await _handler.Handle(purchaseOrderResponse);

            // Assert
            _supportNotificationServiceMock.Verify(b => 
                b.SendSupportErrorNotification(costNumber, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Handle_whenException_ShouldSendEmailToSupportAndRethrowException()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;

            var costNumber = "test cost number";
            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = costNumber,
                ClientName = BuType.Pg.ToString()
            };
            _consumerMock.Setup(c => c.Consume(It.IsAny<PurchaseOrderResponse>())).Throws<Exception>();

            // Act/Assert
            _handler.Awaiting(h => h.Handle(purchaseOrderResponse)).ShouldThrow<Exception>();

            _supportNotificationServiceMock.Verify(b =>
                b.SendSupportErrorNotification(costNumber, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Handle_always_ShouldCallConsumer()
        {
            // Arrange
            var activityType = ActivityTypes.Updated;

            var costNumber = "test cost number";
            var purchaseOrderResponse = new PurchaseOrderResponse
            {
                ActivityType = activityType,
                CostNumber = costNumber,
                ClientName = BuType.Pg.ToString()
            };

            // Act
            await _handler.Handle(purchaseOrderResponse);

            // Assert
            _consumerMock.Verify(c => c.Consume(purchaseOrderResponse), Times.Once);
        }
    }
}

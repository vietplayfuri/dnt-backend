namespace costs.net.messaging.test
{
    using Autofac;
    using core.Models.Utils;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;
    using Serilog;

    [TestFixture]
    public class InternalMessageReceiverTest
    {
        private InternalMessageReceiver _messageReceiver;

        [Test]
        public void Ctor_Always_ShouldUseInternalAmqHost()
        {
            // Arrange
            const string hostName = "amqp://user:password@internal:5672";
            var optionsMock = new Mock<IOptions<AmqSettings>>();
            optionsMock.Setup(o => o.Value).Returns(new AmqSettings
            {
                AmqHost = hostName
            });
            var loggerMock = new Mock<ILogger>();
            var lifeTimeScopeMock = new Mock<ILifetimeScope>();

            // Act
            _messageReceiver = new InternalMessageReceiver(optionsMock.Object, loggerMock.Object, lifeTimeScopeMock.Object);

            // Assert
            _messageReceiver.HostName.Should().Be(hostName);
        }
    }
}

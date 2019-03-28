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
    public class ExternalMessageReceiverTest
    {
        private ExternalMessageReceiver _messageReceiver;

        [Test]
        public void Ctor_Always_ShouldUseInternalAmqHost()
        {
            // Arrange
            const string hostName = "amqp://user:password@external:5672";
            var optionsMock = new Mock<IOptions<AmqSettings>>();
            optionsMock.Setup(o => o.Value).Returns(new AmqSettings
            {
                AmqHostExternal = hostName
            });
            var loggerMock = new Mock<ILogger>();
            var lifeTimeScopeMock = new Mock<ILifetimeScope>();

            // Act
            _messageReceiver = new ExternalMessageReceiver(optionsMock.Object, loggerMock.Object, lifeTimeScopeMock.Object);

            // Assert
            _messageReceiver.HostName.Should().Be(hostName);
        }
    }
}

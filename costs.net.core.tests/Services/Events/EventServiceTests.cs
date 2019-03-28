namespace costs.net.core.tests.Services.Events
{
    using System;
    using System.Threading.Tasks;
    using core.Events;
    using core.Services.Events;
    using Messaging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EventServiceTests
    {
        [SetUp]
        public void Init()
        {
            _amqBus = new Mock<IInternalMessageSender>();
            _eventService = new EventService(_amqBus.Object);
        }

        private const string TestQueue = "test.queue";

        [OneTimeSetUp]
        public void OneTimeInit()
        {
            EventQueueMap.Register<TestEvent>(TestQueue);
        }

        private class TestEvent : BaseEvent
        {
            public TestEvent(Guid id) : base(id)
            {}
        }

        private Mock<IInternalMessageSender> _amqBus;
        private IEventService _eventService;

        [Test]
        public async Task SendEvent_always_shoudSendEventToAmqOnce()
        {
            // Arrange
            var id = Guid.NewGuid();
            var evnt = new TestEvent(id);

            // Act
            await _eventService.SendAsync(evnt);

            // Assert
            _amqBus.Verify(amq => amq.SendMessage(It.Is<TestEvent>(m => m.AggregateId == id), TestQueue), Times.Once);
        }
    }
}
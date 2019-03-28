namespace costs.net.messaging.integration.tests.Stubs
{
    using System.Threading;
    using System.Threading.Tasks;
    using core.Messaging;
    using Handlers;

    public class MessageHandler : IMessageHandler<TestMessage>
    {
        private readonly IMessageStorage _messageStorage;

        public MessageHandler(IMessageStorage messageStorage)
        {
            _messageStorage = messageStorage;
        }

        public async Task Handle(TestMessage message)
        {
            await Task.Delay(10);

            message.Text = $"Thread id {Thread.CurrentThread.ManagedThreadId}";

            _messageStorage.Messages.Enqueue(message);
        }
    }
}

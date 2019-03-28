namespace costs.net.messaging.integration.tests.Stubs
{
    using System.Collections.Concurrent;

    public class MessageStorage : IMessageStorage
    {
        public MessageStorage()
        {
            Messages = new ConcurrentQueue<TestMessage>();
        }
        public ConcurrentQueue<TestMessage> Messages { get; private set; }
    }
}
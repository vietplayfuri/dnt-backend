namespace costs.net.messaging.integration.tests.Stubs
{
    using System.Collections.Concurrent;

    public interface IMessageStorage
    {
        ConcurrentQueue<TestMessage> Messages { get; }
    }
}
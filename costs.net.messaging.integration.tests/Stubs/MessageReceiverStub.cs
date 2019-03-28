namespace costs.net.messaging.integration.tests.Stubs
{
    using Autofac;
    using Serilog;

    public class MessageReceiverStub : MessageReceiver
    {
        private readonly TestAmqSettings _testSettings;

        public MessageReceiverStub(TestAmqSettings testSettings, ILogger logger, ILifetimeScope lifetimeScope)
            : base(testSettings.AmqHost, testSettings, logger, lifetimeScope)
        {
            _testSettings = testSettings;
        }

        protected override void OnConnected()
        {
            base.OnConnected();

            Listen<TestMessage>(_testSettings.QueueName);
        }
    }
}
namespace costs.net.messaging.integration.tests.Stubs
{
    using Microsoft.Extensions.Options;
    using Serilog;

    public class MessageSenderStub : MessageSender
    {
        public MessageSenderStub(IOptions<TestAmqSettings> testSettings, ILogger logger)
            : base(testSettings.Value.AmqHost, testSettings, logger)
        { }
    }
}
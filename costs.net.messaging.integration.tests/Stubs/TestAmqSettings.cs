namespace costs.net.messaging.integration.tests.Stubs
{
    using core.Models.Utils;

    public class TestAmqSettings : AmqSettings
    {
        public string QueueName { get; set; }
    }
}
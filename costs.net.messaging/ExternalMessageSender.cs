namespace costs.net.messaging
{
    using Autofac;
    using core.Messaging;
    using core.Messaging.Messages;
    using core.Models.AMQ;
    using core.Models.Utils;
    using Serilog;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// https://jira.adstream.com/browse/ADC-2681 - We will take it down after this ticker ADC-2681 is done
    /// </summary>
    public class ExternalMessageSender : MessageSender, IExternalMessageSender
    {
        public ExternalMessageSender(IOptions<AmqSettings> amqOptions, ILogger logger, ILifetimeScope lifetimeScope)
            : base(amqOptions.Value.AmqHostExternal, amqOptions, logger)
        { }
    }
}

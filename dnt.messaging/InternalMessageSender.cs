namespace costs.net.messaging
{
    using Autofac;
    using core.Messaging;
    using core.Models.Utils;
    using Microsoft.Extensions.Options;
    using Serilog;

    public class InternalMessageSender : MessageSender, IInternalMessageSender
    {
        public InternalMessageSender(IOptions<AmqSettings> amqOptions, ILogger logger, ILifetimeScope lifetimeScope)
            : base(amqOptions.Value.AmqHost, amqOptions, logger)
        { }
    }
}
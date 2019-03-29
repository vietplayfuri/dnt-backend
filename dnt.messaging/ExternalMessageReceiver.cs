namespace costs.net.messaging
{
    using Autofac;
    using core.Messaging;
    using core.Messaging.Messages;
    using core.Models.AMQ;
    using core.Models.Utils;
    using Serilog;
    using Microsoft.Extensions.Options;

    public class ExternalMessageReceiver : MessageReceiver, IExternalMessageReceiver
    {
        public ExternalMessageReceiver(IOptions<AmqSettings> amqOptions, ILogger logger, ILifetimeScope lifetimeScope)
            : base(amqOptions.Value.AmqHostExternal, amqOptions.Value, logger, lifetimeScope)
        {}

        protected override void OnConnected()
        {
            base.OnConnected();

            Listen<A5EventObject>(AmqSettings.AmqA5Queue);
            Listen<PendingApprovalsRequest>(AmqSettings.BatchUpdateRequest);
            Listen<PurchaseOrderResponse>(AmqSettings.PurchaseOrderQueue);
            Listen<PurchaseOrderErrorResponse>(AmqSettings.XmgErrorQueue);
            Listen<UserLoginEvent>(AmqSettings.A5UserLoginQueue);
        }
    }
}
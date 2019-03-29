namespace costs.net.messaging
{
    using System;
    using Apache.NMS;
    using Autofac;
    using core.Messaging;
    using core.Models.Utils;
    using Newtonsoft.Json;
    using Serilog;

    public class MessageReceiver : BaseBus, IMessageReceiver
    {
        private readonly ILifetimeScope _lifetimeScope;

        private readonly ILogger _logger;

        public MessageReceiver(string hostName, AmqSettings amqSettings, ILogger logger, ILifetimeScope lifetimeScope)
            : base(hostName, amqSettings, logger)
        {
            _logger = logger;
            _lifetimeScope = lifetimeScope;
        }

        public void Listen<T>(string queueTopic)
            where T : new()
        {
            var handler = _lifetimeScope.Resolve<ISafeHandler<T>>();
            CreateSession();
            var dest = Session.GetQueue(queueTopic);
            AddDisposableResource(dest);

            _logger.Information($"Listening to {queueTopic}");

            var consumer = Session.CreateConsumer(dest);
            AddDisposableResource(consumer);
            consumer.Listener += async message =>
            {
                try
                {
                    var queue = message.NMSDestination;
                    var body = JsonConvert.DeserializeObject<T>((message as ITextMessage)?.Text);
                    _logger.Information($"Received Message of type {typeof(T)}, from Queue: {queue}, Message: {body}");
                    await handler.Handle(body);
                }
                catch (Exception e)
                {
                    var text = (message as ITextMessage)?.Text;
                    _logger.Error($"Error processing message from queue: [{message.NMSDestination}] message body: [{text}]");
                    _logger.Error(e, e.Message);
                }
                finally
                {
                    message.Acknowledge();
                }
            };
        }
    }
}

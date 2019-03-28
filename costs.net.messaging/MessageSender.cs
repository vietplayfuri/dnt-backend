namespace costs.net.messaging
{
    using System;
    using System.Threading.Tasks;
    using Apache.NMS;
    using core.Messaging;
    using core.Models.Utils;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Serilog;

    public class MessageSender : BaseBus, IMessageSender
    {
        private readonly ILogger _logger;
        private const string CostSender = "costs-sender";
        private const string Sender = "Sender";

        public MessageSender(string hostName, IOptions<AmqSettings> amqSettings, ILogger logger)
            : base(hostName, amqSettings.Value, logger)
        {
            _logger = logger;
        }

        public async Task SendMessage<T>(T message, string queueTopic) where T : class
        {
            _logger.Information($"Sending message of type {typeof(T)} to '{queueTopic}' queue: {JsonConvert.SerializeObject(message)}");
            await ActivateAsync();
            using (var session = Connection.CreateSession())
            {
                using (var dest = session.GetQueue(queueTopic))
                {
                    using (var producer = session.CreateProducer(dest))
                    {
                        var msg = session.CreateTextMessage(JsonConvert.SerializeObject(message));
                        msg.Properties[Sender] = CostSender;
                        producer.Send(msg);
                    }
                }
            }
        }
    }
}

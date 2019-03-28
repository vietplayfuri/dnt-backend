namespace costs.net.messaging.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using core.Builders;
    using core.Builders.Notifications;
    using core.Exceptions;
    using core.ExternalResource.Paperpusher;
    using core.Messaging;
    using core.Messaging.Messages;
    using core.Models;
    using core.Services.Notifications;
    using core.Services.PurchaseOrder;
    using Newtonsoft.Json;
    using Serilog;

    public class PurchaseOrderResponseHandler : IMessageHandler<PurchaseOrderResponse>
    {
        private readonly ISupportNotificationService _supportNotificationService;
        private readonly ILogger _logger;
        private readonly IEnumerable<Lazy<IPurchaseOrderResponseConsumer, PluginMetadata>> _purchaseOrderConsumers;

        public PurchaseOrderResponseHandler(ILogger logger,
            IEnumerable<Lazy<IPurchaseOrderResponseConsumer, PluginMetadata>> purchaseOrderConsumers,
            ISupportNotificationService supportNotificationService
        )
        {
            _logger = logger;
            _purchaseOrderConsumers = purchaseOrderConsumers;
            _supportNotificationService = supportNotificationService;
        }

        public async Task Handle(PurchaseOrderResponse message)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            try
            {
                _logger.Information($"Consuming purchase order message: {messageJson}");

                var buType = GetBuType(message.ClientName);

                var consumer = _purchaseOrderConsumers.First(b => b.Metadata.BuType == buType).Value;
                await consumer.Consume(message);
            }
            catch (XmgException ex)
            {
                await OnTechnicalError(message.CostNumber, ex.Message);
            }
            catch (Exception ex)
            {
                var errorText = $"Failed to consume response message: {messageJson} from XMG {ex}";
                await OnTechnicalError(message.CostNumber, errorText);
                throw;
            }
        }

        private async Task OnTechnicalError(string costNumber, string errorText)
        {
            _logger.Error(errorText);

            // Send email to support team
            await _supportNotificationService.SendSupportErrorNotification(costNumber, errorText);
        }

        private static BuType GetBuType(string clientName)
        {
            if (!Enum.TryParse(clientName, out BuType buType))
            {
                throw new XmgException($"Unknown client '{clientName}'") { ClientName = clientName };
            }
            return buType;
        }
    }
}


namespace costs.net.messaging.Handlers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Exceptions;
    using core.Messaging;
    using core.Messaging.Messages;
    using core.Models;
    using core.Models.User;
    using core.Services.Costs;
    using core.Services.Notifications;
    using dataAccess;
    using dataAccess.Entity;
    using Elasticsearch.Net;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using Newtonsoft.Json;

    public class PurchaseOrderErrorResponseHandler : IMessageHandler<PurchaseOrderErrorResponse>
    {
        private readonly IApprovalService _approvalService;
        private readonly EFContext _efContext;
        private readonly ISupportNotificationService _supportNotificationService;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly ILogger _logger;

        public PurchaseOrderErrorResponseHandler(
            EFContext efContext,
            IApprovalService approvalService,
            ILogger logger,
            IEmailNotificationService emailNotificationService,
            ISupportNotificationService supportNotificationService
        )
        {
            _efContext = efContext;
            _approvalService = approvalService;
            _logger = logger;
            _supportNotificationService = supportNotificationService;
            _emailNotificationService = emailNotificationService;
        }

        public async Task Handle(PurchaseOrderErrorResponse message)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            _logger.Information($"Consuming purchase order error message: {messageJson}");

            var errorMessage = JsonConvert.DeserializeObject<PurchaseOrderResponseError>(message.Payload.ToString())
                .ErrorMessages
                .FirstOrDefault();

            switch (errorMessage.Type)
            {
                case ResponseErrorType.AwaitingActionInCosts:
                    _logger.Warning($"There is an issue with purchase order for cost {message.CostNumber}. Awaiting action in cost module: {errorMessage.Message}");
                    await Reject(message, errorMessage);
                    break;
                case ResponseErrorType.AwaitingActionsInExternalSystem:
                    await OnBusinessError(message.CostNumber,
                        $"There is an issue with purchase order for cost {message.CostNumber}. Awaiting action in Coupa: {errorMessage.Message}");
                    break;
                case ResponseErrorType.DownUp:
                    await OnBusinessError(message.CostNumber,
                        $"There is an issue with purchase order for cost {message.CostNumber}. Coupa is Down/Up: {errorMessage.Message}");
                    break;
                case ResponseErrorType.Technical:
                    await OnTechnicalError(message.CostNumber,
                        $"There is a technical issue with purchase order for cost {message.CostNumber}. Error type '{errorMessage.Type}': {errorMessage.Message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task OnBusinessError(string costNumber, string errorText)
        {
            _logger.Warning(errorText);

            var cost = await _efContext.Cost.Include(c => c.Owner).SingleAsync(c => c.CostNumber == costNumber);

            // Send email to cost owner
            await _supportNotificationService.SendGenericErrorNotification(cost.CostNumber, errorText, cost.Owner.GdamUserId);
        }

        private async Task OnTechnicalError(string costNumber, string errorText)
        {
            _logger.Error(errorText);

            // Send email to support team
            await _supportNotificationService.SendSupportErrorNotification(costNumber, errorText);

        }

        private async Task Reject(PurchaseOrderResponse message, PurchaseOrderErrorMessage errorMessage)
        {
            var adminUser = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Email == ApprovalMemberModel.BrandApprovalUserEmail);
            var adminUserIdentity = new SystemAdminUserIdentity(adminUser);
            var cost = await _efContext.Cost.SingleAsync(c => c.CostNumber == message.CostNumber);
            var result = await _approvalService.Reject(cost.Id, adminUserIdentity, GetBuType(message.ClientName), errorMessage.Message, SourceSystem.Coupa);
            if (!result.Success)
            {
                await OnTechnicalError(message.CostNumber, $"Failed to reject cost due to error! {result.Messages}");
            }
            else
            {
                var error = $"Cost {message.CostNumber} rejected due to : {errorMessage.Type.GetStringValue()} {errorMessage.Message}";
                _logger.Information(error);

                await _emailNotificationService.CostHasBeenRejected(
                    cost.Id, adminUser.Id, ApprovalType.Brand.ToString(), error);
            }
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

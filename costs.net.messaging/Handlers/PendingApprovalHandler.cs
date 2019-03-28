namespace costs.net.messaging.Handlers
{
    using System.Threading.Tasks;
    using core.Messaging;
    using core.Messaging.Messages;
    using core.Services.Costs;

    public class PendingApprovalHandler : IMessageHandler<PendingApprovalsRequest>
    {
        private readonly ICostService _costService;

        public PendingApprovalHandler(ICostService costService)
        {
            _costService = costService;
        }

        public async Task Handle(PendingApprovalsRequest message)
        {
            await _costService.SendPendingApprovalsFor(message.ClientName);
        }
    }
}

namespace costs.net.plugins.PG.Services.PurchaseOrder
{
    using core.Events;
    using core.Events.Cost;
    using core.ExternalResource.Paperpusher;
    using core.Messaging.Messages;
    using core.Services.PurchaseOrder;
    using dataAccess;
    using dataAccess.Entity;
    using Serilog;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using costs.net.core.Services.Costs;
    using costs.net.core.Builders.Response.Cost;
    using costs.net.plugins.PG.Models.Stage;

    public class PgPaperpusherNotifier : IPaperpusherNotifier
    {
        private readonly ILogger _logger;
        private readonly IPgPurchaseOrderService _purchaseOrderService;
        private readonly IPaperpusherClient _paperpusherClient;
        private readonly ICostService _costService;

        private readonly Dictionary<CostStageRevisionStatus, string> _paperPusherActivityTypeMap = new Dictionary<CostStageRevisionStatus, string>
        {
            { CostStageRevisionStatus.PendingBrandApproval, ActivityTypes.Submitted },
            { CostStageRevisionStatus.PendingCancellation, ActivityTypes.Cancelled },
            { CostStageRevisionStatus.PendingRecall, ActivityTypes.Recalled },
            { CostStageRevisionStatus.Approved, ActivityTypes.GoodsReceiptSubmitted }
        };

        /// <summary>
        /// https://jira.adstream.com/browse/ADC-2687 - We will take it down after this ticker ADC-2681 is done
        /// Cost number and their target stages
        /// </summary>
        private readonly Dictionary<string, List<string>> _unSentCostList = new Dictionary<string, List<string>> {
            { "PRO0001944V0006", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0002065S0001", new List<string> { nameof(CostStages.OriginalEstimate),nameof(CostStages.FirstPresentation) } },
            { "PRO0002130V0001", new List<string> {nameof(CostStages.OriginalEstimate) } },
            { "PRO0001945V0001", new List<string> { nameof(CostStages.OriginalEstimate),nameof(CostStages.FirstPresentation) } },
            { "PRO0001828V0001", new List<string> { nameof(CostStages.OriginalEstimate),nameof(CostStages.FirstPresentation) } },
            { "PRO0001755V0001", new List<string> { nameof(CostStages.OriginalEstimate),nameof(CostStages.FirstPresentation) } },
            { "PRO0001949V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001750V0007", new List<string> {nameof(CostStages.OriginalEstimate) } },
            { "PRO0002086V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001567S0002", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0024V0000006", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001567V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001712S0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001944V0003", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001820V0004", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001864V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001944V0004", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001945V0002", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001742V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0002213V0001", new List<string> {nameof(CostStages.OriginalEstimate) } },
            { "PRO0002182S0001", new List<string> {nameof(CostStages.OriginalEstimate) } },
            { "PRO0001769V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0001789V0002", new List<string> {nameof(CostStages.OriginalEstimate) } },
            { "PRO0002191V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0002040S0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } },
            { "PRO0002043V0001", new List<string> { nameof(CostStages.OriginalEstimate), nameof(CostStages.FirstPresentation) } }
        };

        public PgPaperpusherNotifier(
            ILogger logger,
            IPgPurchaseOrderService purchaseOrderService,
            IPaperpusherClient paperpusherClient,
            ICostService costService
            )
        {
            _logger = logger;
            _purchaseOrderService = purchaseOrderService;
            _paperpusherClient = paperpusherClient;
            _costService = costService;
        }

        public async Task<bool> Notify(CostStageRevisionStatusChanged evnt)
        {
            if (!_paperPusherActivityTypeMap.ContainsKey(evnt.Status))
            {
                return false;
            }

            // Should this logic be in rules engine?
            var activityType = _paperPusherActivityTypeMap[evnt.Status];
            if (!await _purchaseOrderService.NeedToSendPurchaseOrder(evnt))
            {
                return false;
            }

            var message = await _purchaseOrderService.GetPurchaseOrder(evnt);

            // https://jira.adstream.com/browse/ADC-2687 - We will take it down after this ticker ADC-2681 is done
            if (message != null && !string.IsNullOrWhiteSpace(message.CostNumber) && _unSentCostList.ContainsKey(message.CostNumber))
            {
                var cost = await _costService.GetCostByCostNumber(message.CostNumber);

                //Do not send to coupa in case cost numbers are in excel file and their latest stages are the same with target stage in excel file
                if (cost != null && cost.LatestCostStageRevision != null && _unSentCostList[message.CostNumber].Contains(cost.LatestCostStageRevision.Name))
                {
                    _logger.Information($"Activity {activityType}. Stop sending notification to Coupa for cost with id {evnt.AggregateId} and cost number {message.CostNumber} because of ADC-2681");
                    return true;
                }
            }

            _logger.Information($"Activity {activityType}. " +
                                $"Sending notification to Coupa for cost with id {evnt.AggregateId} and cost number {message.CostNumber} " +
                                $"stage revision {evnt.CostStageRevisionId} and status {evnt.Status}. " +
                                $"Message: {JsonConvert.SerializeObject(message)}");

            return await NotifyExternalSystem(evnt, message, activityType);
        }

        private async Task<bool> NotifyExternalSystem<TMessage>(IEvent evnt, TMessage message, string activityType)
            where TMessage : class
        {
            return await _paperpusherClient.SendMessage(evnt.AggregateId, message, activityType);
        }
    }
}

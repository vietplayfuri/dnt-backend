namespace costs.net.messaging.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using core.Builders;
    using core.Events.Cost;
    using core.Messaging;
    using core.Services.PurchaseOrder;
    using core.Services.Search;
    using Serilog;

    public class CostStageRevisionStatusChangedHandler : IMessageHandler<CostStageRevisionStatusChanged>
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger _logger;
        private readonly IEnumerable<Lazy<IPaperpusherNotifier, PluginMetadata>> _paperpusherNotifiers;

        public CostStageRevisionStatusChangedHandler(
            ILogger logger,
            IEnumerable<Lazy<IPaperpusherNotifier, PluginMetadata>> paperpusherNotifiers,
            IElasticSearchService elasticSearchService)
        {
            _logger = logger;
            _paperpusherNotifiers = paperpusherNotifiers;
            _elasticSearchService = elasticSearchService;
        }

        public async Task Handle(CostStageRevisionStatusChanged evnt)
        {
            _logger.Information($"Status of cost stage revision {evnt.CostStageRevisionId} of cost {evnt.AggregateId} has been changed to {evnt.Status}");

            await _elasticSearchService.UpdateCostSearchItem(evnt);

            var paperpusherNotifier = _paperpusherNotifiers.First(n => n.Metadata.BuType == evnt.BuType).Value;
            await paperpusherNotifier.Notify(evnt);
        }
    }
}

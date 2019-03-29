namespace costs.net.messaging.Handlers.Events
{
    using System;
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostOwnerChangedHandler : IMessageHandler<CostOwnerChanged>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostOwnerChangedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public async Task Handle(CostOwnerChanged costOwnerChanged)
        {
            if (costOwnerChanged == null)
            {
                throw new ArgumentNullException(nameof(costOwnerChanged));
            }

            await _elasticSearchService.UpdateCostSearchItem(costOwnerChanged);
        }
    }
}

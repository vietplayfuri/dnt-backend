namespace costs.net.messaging.Handlers.Events
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class ExpectedAssetUpdatedHandler : IMessageHandler<ExpectedAssetUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ExpectedAssetUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }
        public Task Handle(ExpectedAssetUpdated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

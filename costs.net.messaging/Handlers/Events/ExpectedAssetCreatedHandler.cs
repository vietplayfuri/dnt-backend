namespace costs.net.messaging.Handlers.Events
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class ExpectedAssetCreatedHandler : IMessageHandler<ExpectedAssetCreated>
    {
        private IElasticSearchService _elasticSearchService;

        public ExpectedAssetCreatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }
        public Task Handle(ExpectedAssetCreated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

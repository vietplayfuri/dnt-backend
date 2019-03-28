namespace costs.net.messaging.Handlers.Events
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class ExpectedAssetDeletedHandler : IMessageHandler<ExpectedAssetDeleted>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ExpectedAssetDeletedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }
        public Task Handle(ExpectedAssetDeleted evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

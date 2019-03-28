namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class ProductionDetailsUpdatedHandler : IMessageHandler<ProductionDetailsUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ProductionDetailsUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(ProductionDetailsUpdated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

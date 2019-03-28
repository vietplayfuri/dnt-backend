namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostLineItemsUpdatedHandler : IMessageHandler<CostLineItemsUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostLineItemsUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostLineItemsUpdated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

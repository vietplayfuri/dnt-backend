namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;
    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostUpdatedHandler : IMessageHandler<CostUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostUpdated message)
        {
            return _elasticSearchService.UpdateCostSearchItem(message);
        }
    }
}

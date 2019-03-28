namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostCreatedHandler : IMessageHandler<CostCreated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostCreatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostCreated evnt)
        {
            return _elasticSearchService.CreateCostSearchItem(evnt);
        }
    }
}

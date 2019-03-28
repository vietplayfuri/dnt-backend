namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostDeletedHandler : IMessageHandler<CostDeleted>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostDeletedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostDeleted costDeleted)
        {
            return _elasticSearchService.DeleteItem<CostSearchItem>(costDeleted.AggregateId);
        }
    }
}

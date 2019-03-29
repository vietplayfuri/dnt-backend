namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class StageDetailsUpdatedHandler : IMessageHandler<StageDetailsUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public StageDetailsUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(StageDetailsUpdated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

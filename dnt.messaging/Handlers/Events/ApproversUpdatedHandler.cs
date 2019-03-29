namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class ApproversUpdatedHandler : IMessageHandler<ApproversUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ApproversUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public async Task Handle(ApproversUpdated evnt)
        {
            await _elasticSearchService.UpdateCostSearchItem(evnt);
        }
    }
}

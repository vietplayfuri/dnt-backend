namespace costs.net.messaging.Handlers.Events.CostUser
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.CostUser;
    using core.Messaging;
    using core.Services.Search;

    public class CostUsersUpdatedHandler : IMessageHandler<CostUsersUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostUsersUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostUsersUpdated evnt)
        {
            return _elasticSearchService.UpdateMultipleItems<CostUsersUpdated, CostUserSearchItem>(evnt);
        }
    }
}

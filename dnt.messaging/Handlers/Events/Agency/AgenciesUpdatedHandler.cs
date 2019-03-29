namespace costs.net.messaging.Handlers.Events.Agency
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.Agency;
    using core.Messaging;
    using core.Services.Search;

    public class AgenciesUpdatedHandler : IMessageHandler<AgenciesUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public AgenciesUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(AgenciesUpdated evnt)
        {
            return _elasticSearchService.UpdateMultipleItems<AgenciesUpdated, AgencySearchItem>(evnt);
        }
    }
}

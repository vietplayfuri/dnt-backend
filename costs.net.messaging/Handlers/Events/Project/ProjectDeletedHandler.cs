namespace costs.net.messaging.Handlers.Events.Project
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.Project;
    using core.Messaging;
    using core.Services.Search;

    public class ProjectDeletedHandler : IMessageHandler<ProjectDeleted>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ProjectDeletedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(ProjectDeleted evnt)
        {
            return _elasticSearchService.DeleteItem<ProjectSearchItem>(evnt.AggregateId);
        }
    }
}

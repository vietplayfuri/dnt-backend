namespace costs.net.messaging.Handlers.Events.Project
{
    using System.Threading.Tasks;

    using core.Events.Project;
    using core.Messaging;
    using core.Services.Search;

    public class ProjectCreatedHandler : IMessageHandler<ProjectCreated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ProjectCreatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(ProjectCreated evnt)
        {
            return _elasticSearchService.CreateProjectSearchItem(evnt);
        }
    }
}

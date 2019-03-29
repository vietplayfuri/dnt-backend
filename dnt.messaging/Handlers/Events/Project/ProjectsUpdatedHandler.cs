namespace costs.net.messaging.Handlers.Events.Project
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.Project;
    using core.Messaging;
    using core.Services.Search;

    public class ProjectsUpdatedHandler : IMessageHandler<ProjectsUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ProjectsUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(ProjectsUpdated evnt)
        {
            return _elasticSearchService.UpdateMultipleItems<ProjectsUpdated, ProjectSearchItem>(evnt);
        }
    }
}

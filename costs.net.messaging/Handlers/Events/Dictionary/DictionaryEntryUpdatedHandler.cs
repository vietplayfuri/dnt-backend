
namespace costs.net.messaging.Handlers.Events.Dictionary
{
    using System.Threading.Tasks;

    using core.Builders.Response;
    using core.Events.Dictionary;
    using core.Messaging;
    using core.Services.Search;

    public class DictionaryEntryUpdatedHandler : IMessageHandler<DictionaryEntryUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public DictionaryEntryUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(DictionaryEntryUpdated evnt)
        {
            return _elasticSearchService.UpdateMultipleItems<DictionaryEntryUpdated, DictionaryEntrySearchItem>(evnt);
        }
    }
}

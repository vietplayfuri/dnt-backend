using System;
using System.Collections.Generic;
using System.Text;

namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Services.Search;
    using costs.net.core.Events.SupportingDocument;

    public class SupportingDocumentUpdatedHandler : MessageHandler<SupportingDocumentUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public SupportingDocumentUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }
        public override Task Handle(SupportingDocumentUpdated message)
        {
           return _elasticSearchService.UpdateCostSearchItem(message);
        }
    }
}

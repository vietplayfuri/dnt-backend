﻿namespace costs.net.messaging.Handlers.Events
{
    using System.Threading.Tasks;

    using core.Events.Cost;
    using core.Messaging;
    using core.Services.Search;

    public class CostsUpdatedHandler : IMessageHandler<CostsUpdated>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public CostsUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(CostsUpdated evnt)
        {
            return _elasticSearchService.UpdateCostSearchItems(evnt);
        }
    }
}

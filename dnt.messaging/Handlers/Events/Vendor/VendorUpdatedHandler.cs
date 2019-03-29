namespace costs.net.messaging.Handlers.Events.Vendor
{
    using System.Threading.Tasks;
    using core.Events.Vendor;
    using core.Messaging;
    using core.Services.Search;

    public class VendorUpdatedHandler : IMessageHandler<VendorUpserted>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public VendorUpdatedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(VendorUpserted vendorUpdated)
        {
            return _elasticSearchService.UpdateVendor(vendorUpdated.AggregateId);
        }
    }
}

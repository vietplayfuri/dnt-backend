namespace costs.net.messaging.Handlers.Events.Vendor
{
    using System.Threading.Tasks;
    using core.Events.Vendor;
    using core.Messaging;
    using core.Services.Search;

    public class VendorDeletedHandler : IMessageHandler<VendorDeleted>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public VendorDeletedHandler(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Handle(VendorDeleted vendorDeleted)
        {
            return _elasticSearchService.UpdateVendor(vendorDeleted.AggregateId);
        }
    }
}

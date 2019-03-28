namespace costs.net.messaging
{
    using Autofac;
    using core.Events;
    using core.Events.Agency;
    using core.Events.Cost;
    using core.Events.CostUser;
    using core.Events.Dictionary;
    using core.Events.Project;
    using core.Events.SupportingDocument;
    using core.Events.Vendor;
    using core.Messaging;
    using core.Models.Utils;
    using Serilog;
    using Microsoft.Extensions.Options;

    public class InternalMessageReceiver : MessageReceiver, IInternalMessageReceiver
    {
        public InternalMessageReceiver(IOptions<AmqSettings> amqOptions, ILogger logger, ILifetimeScope lifetimeScope)
            : base(amqOptions.Value.AmqHost, amqOptions.Value, logger, lifetimeScope)
        {}

        protected override void OnConnected()
        {
            base.OnConnected();

            // Events
            Listen<ApproversUpdated>();
            Listen<CostCreated>();
            Listen<CostDeleted>();
            Listen<CostStageRevisionStatusChanged>();
            Listen<CostLineItemsUpdated>();
            Listen<CostsUpdated>();
            Listen<ExpectedAssetCreated>();
            Listen<ExpectedAssetDeleted>();
            Listen<ExpectedAssetUpdated>();
            Listen<ProductionDetailsUpdated>();
            Listen<StageDetailsUpdated>();
            Listen<ProjectCreated>();
            Listen<ProjectsUpdated>();
            Listen<ProjectDeleted>();
            Listen<AgenciesUpdated>();
            Listen<CostUsersUpdated>();
            Listen<DictionaryEntryUpdated>();
            Listen<CostUpdated>();
            Listen<SupportingDocumentUpdated>();
            Listen<CostOwnerChanged>();
            Listen<VendorUpserted>();
            Listen<VendorDeleted>();
        }

        private void Listen<T>()
            where T : BaseEvent, new()
        {
            Listen<T>(EventQueueMap.GetQueueName<T>());
        }
    }
}

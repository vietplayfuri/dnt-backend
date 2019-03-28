using System;
using System.Collections.Generic;
using AutoMapper;
using costs.net.core.Helpers;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.dataAccess.Entity;
using Serilog;

namespace costs.net.plugins.PG.Builders.Notifications
{
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;

    class ReopenApprovedNotificationBuilder : NotificationBuilderBase
    {
        private static readonly ILogger Logger = Log.ForContext<RejectedNotificationBuilder>();

        internal ReopenApprovedNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(dataAccess.Entity.Cost cost, CostNotificationUsers costUsers, string approvedByName, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Cost Owner
            var costOwner = costUsers.CostOwner;
            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.ReopenApproved, costOwner.GdamUserId);
            AddSharedTo(costOwnerNotification);

            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            PopulateOtherFields(costOwnerNotification, string.Empty, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            costOwnerNotification.Object.Approver.Name = approvedByName;
            notifications.Add(costOwnerNotification);

            return notifications;
        }
        
    }
}

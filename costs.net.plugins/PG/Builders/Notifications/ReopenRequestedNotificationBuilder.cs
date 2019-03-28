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
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;

    class ReopenRequestedNotificationBuilder : NotificationBuilderBase
    {
        private static readonly ILogger Logger = Log.ForContext<RejectedNotificationBuilder>();

        internal ReopenRequestedNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(dataAccess.Entity.Cost cost,
            CostNotificationUsers costUsers, string requestedByName, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Admin Notification - one notification object for all admins
            var adminIds = costUsers.ClientAdmins.Select(x => x.GdamUserId).ToList();
            
            var adminNotification = new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.ReopenRequested, adminIds);
            AddSharedTo(adminNotification);
            MapEmailNotificationObject(adminNotification.Object, cost, costUsers.CostOwner);
            PopulateOtherFields(adminNotification, string.Empty, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(adminNotification.Object, cost.Id);
            adminNotification.Object.Approver.Name = requestedByName;
            notifications.Add(adminNotification);

            await AddFinanceManagerNotification(core.Constants.EmailNotificationActionType.CostStatus, cost, costUsers, costStageRevision, timestamp, notifications);

            return notifications;
        }
        
    }
}

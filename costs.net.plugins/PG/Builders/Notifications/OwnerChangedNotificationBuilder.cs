namespace costs.net.plugins.PG.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using Cost = dataAccess.Entity.Cost;

    internal class OwnerChangedNotificationBuilder : NotificationBuilderBase
    {
        internal OwnerChangedNotificationBuilder(
            IMapper mapper,
            IApplicationUriHelper uriHelper,
            ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        { }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(
            Cost cost,
            CostNotificationUsers costUsers,
            CostStageRevision costStageRevision,
            DateTime timestamp,
            CostUser changeApprover,
            CostUser previousOwner)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();
            string actionType = core.Constants.EmailNotificationActionType.CostOwnerChanged;
            var costOwner = costUsers.CostOwner;

            //add recipients
            var recipients = new List<string> { costOwner.GdamUserId };
            if (costUsers.Watchers != null)
                recipients.AddRange(costUsers.Watchers);
            if (costUsers.Approvers != null)
                recipients.AddRange(costUsers.Approvers);
            recipients.Add(previousOwner.GdamUserId);

            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, recipients.Distinct());
            AddSharedTo(costOwnerNotification);
            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            costOwnerNotification.Object.Cost.PreviousOwner = previousOwner.FullName;
            var approver = costOwnerNotification.Object.Approver;
            approver.Name = changeApprover.FullName;
            PopulateOtherFields(costOwnerNotification, core.Constants.EmailNotificationParents.CostOwner, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            notifications.Add(costOwnerNotification);

            return notifications;
        }
    }
}

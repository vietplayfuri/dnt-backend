namespace costs.net.plugins.PG.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using core;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using Extensions;
    using Models.Stage;
    using Cost = dataAccess.Entity.Cost;

    internal class CancelledNotificationBuilder : NotificationBuilderBase
    {
        internal CancelledNotificationBuilder(
            IMapper mapper,
            IApplicationUriHelper uriHelper,
            ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            IRegionsService regionsService,
            AppSettings appSettings,
            EFContext efContext) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(Cost cost,
            CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Cost Owner
            var costOwner = costUsers.CostOwner;
            var recipients = new List<string> { costOwner.GdamUserId };
            recipients.AddRange(costUsers.Watchers);

            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(Constants.EmailNotificationActionType.Cancelled, recipients);
            AddSharedTo(costOwnerNotification);
            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            PopulateOtherFields(costOwnerNotification, Constants.EmailNotificationParents.CostOwner, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            notifications.Add(costOwnerNotification);

            if (ShouldNotifyInsuranceUsers(costUsers))
            {
                var insuranceUserNotification = new EmailNotificationMessage<CostNotificationObject>(Constants.EmailNotificationActionType.Cancelled, costUsers.InsuranceUsers);
                AddSharedTo(insuranceUserNotification);
                MapEmailNotificationObject(insuranceUserNotification.Object, cost, costOwner);
                PopulateOtherFields(insuranceUserNotification, Constants.EmailNotificationParents.InsuranceUser, timestamp, cost.Id, costStageRevision.Id);
                await PopulateMetadata(insuranceUserNotification.Object, cost.Id);
                notifications.Add(insuranceUserNotification);
            }
            await AddFinanceManagerNotification(Constants.EmailNotificationActionType.Cancelled,
                cost, costUsers, costStageRevision, timestamp, notifications);

            return notifications;
        }
    }
}

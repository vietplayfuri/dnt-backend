using System;
using System.Collections.Generic;
using AutoMapper;
using costs.net.core.Helpers;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.core.Services.Regions;
using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Extensions;
using Serilog;

namespace costs.net.plugins.PG.Builders.Notifications
{
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using dataAccess;

    class RejectedNotificationBuilder : NotificationBuilderBase
    {
        private static readonly ILogger Logger = Log.ForContext<RejectedNotificationBuilder>();

        internal RejectedNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            IRegionsService regionsService,
            AppSettings appSettings,
            EFContext efContext) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(dataAccess.Entity.Cost cost,
            CostNotificationUsers costUsers, string approverName, string approvalType, string comments, CostStageRevision costStageRevision, DateTime timestamp)
        {
               var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();
            const string actionType = core.Constants.EmailNotificationActionType.Rejected;

            //Cost Owner
            var costOwner = costUsers.CostOwner;
            var recipients = new List<string> { costOwner.GdamUserId };
            recipients.AddRange(costUsers.Watchers);

            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, recipients);
            AddSharedTo(costOwnerNotification);
            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            PopulateOtherFields(costOwnerNotification, core.Constants.EmailNotificationParents.CostOwner, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            var obj = costOwnerNotification.Object;
            var approver = obj.Approver;
            approver.Name = approverName;
            approver.Type = approvalType;
            obj.Comments = comments;

            notifications.Add(costOwnerNotification);
            
            if (ShouldNotifyInsuranceUsers(costUsers))
            {
                var insuranceUserNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, costUsers.InsuranceUsers);
                AddSharedTo(insuranceUserNotification);
                MapEmailNotificationObject(insuranceUserNotification.Object, cost, costOwner);
                PopulateOtherFields(insuranceUserNotification, core.Constants.EmailNotificationParents.InsuranceUser, timestamp, cost.Id, costStageRevision.Id);
                await PopulateMetadata(insuranceUserNotification.Object, cost.Id);
                obj = insuranceUserNotification.Object;
                approver = obj.Approver;
                approver.Name = approverName;
                approver.Type = approvalType;
                obj.Comments = comments;

                notifications.Add(insuranceUserNotification);
            }
            await AddFinanceManagerNotification(actionType, cost, costUsers, costStageRevision, timestamp, notifications);

            return notifications;
        }
        
    }
}

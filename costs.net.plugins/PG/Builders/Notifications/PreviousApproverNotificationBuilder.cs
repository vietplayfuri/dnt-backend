namespace costs.net.plugins.PG.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Extensions;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;

    class PreviousApproverNotificationBuilder : NotificationBuilderBase
    {
        internal PreviousApproverNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(IEnumerable<ApprovalMember> removedApprovers,
            dataAccess.Entity.Cost cost, CostNotificationUsers costUsers, CostStageRevision previousRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            foreach (var removedMember in removedApprovers)
            {
                if (removedMember.IsSystemApprover())
                {
                    continue;
                }

                var approvalCostUser = removedMember.CostUser;
                var costOwner = costUsers.CostOwner;
                string actionType = core.Constants.EmailNotificationActionType.ApproverUnassigned;
                string parent = core.Constants.EmailNotificationParents.Approver;
                
                var previousApproverNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, approvalCostUser.GdamUserId);
                AddSharedTo(previousApproverNotification);
                MapEmailNotificationObject(previousApproverNotification.Object, cost, costOwner, approvalCostUser);
                PopulateOtherFields(previousApproverNotification, parent, timestamp, cost.Id, previousRevision.Id);
                await PopulateMetadata(previousApproverNotification.Object, cost.Id);

                var replacement = GetReplacement(cost.LatestCostStageRevision, removedMember);

                var approver = previousApproverNotification.Object.Approver;
                approver.Type = GetApprovalType(removedMember.Approval);
                if (replacement != null)
                {
                    approver.Name = replacement.CostUser.FullName;
                }
                notifications.Add(previousApproverNotification);
            }

            return notifications;
        }

        private static ApprovalMember GetReplacement(CostStageRevision costStageRevision, ApprovalMember removedMember)
        {
            foreach (var approval in costStageRevision.Approvals)
            {
                if (approval.Type == removedMember.Approval.Type)
                {
                    foreach (var approvalMember in approval.ApprovalMembers)
                    {
                        if (approvalMember.IsSystemApprover())
                        {
                            continue;
                        }

                        return approvalMember;
                    }
                }
            }
            return null;
        }
    }
}

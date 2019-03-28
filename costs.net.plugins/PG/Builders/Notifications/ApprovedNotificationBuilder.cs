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
    using Microsoft.EntityFrameworkCore;
    using Cost = dataAccess.Entity.Cost;

    internal class ApprovedNotificationBuilder : NotificationBuilderBase
    {
        internal ApprovedNotificationBuilder(
            IMapper mapper,
            IApplicationUriHelper uriHelper,
            ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(Cost cost,
            CostNotificationUsers costUsers, string approverName, string approvalType, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Cost Owner
            string actionType;
            var parent = core.Constants.EmailNotificationParents.CostOwner;
            if (approvalType == core.Constants.EmailApprovalType.Brand)
            {
                actionType = core.Constants.EmailNotificationActionType.BrandApprovalApproved;
                approvalType = await GetBrandManagerRoleLabel();
                if (cost.IsExternalPurchases)
                {
                    parent = core.Constants.EmailNotificationParents.MyPurchases;
                }
            }
            else
            {
                actionType = core.Constants.EmailNotificationActionType.TechnicalApprovalApproved;
            }
            var costOwner = costUsers.CostOwner;
            var recipients = new List<string> { costOwner.GdamUserId };
            if (costUsers.Watchers != null)
            {
                recipients.AddRange(costUsers.Watchers);
            }
            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, recipients);
            AddSharedTo(costOwnerNotification);
            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            PopulateOtherFields(costOwnerNotification, parent, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            var approver = costOwnerNotification.Object.Approver;
            approver.Name = approverName;
            approver.Type = approvalType;
            notifications.Add(costOwnerNotification);

            if (cost.Status == CostStageRevisionStatus.Approved)
            {
                if (ShouldNotifyInsuranceUsers(costUsers))
                {
                    //Send to all InsuranceUsers as well
                    var insuranceUsers = costUsers.InsuranceUsers;
                    var insuranceUserNotification =
                        new EmailNotificationMessage<CostNotificationObject>(core.Constants.EmailNotificationActionType.AllApprovalsApproved, insuranceUsers);
                    parent = core.Constants.EmailNotificationParents.InsuranceUser;
                    AddSharedTo(insuranceUserNotification);
                    MapEmailNotificationObject(insuranceUserNotification.Object, cost, costUsers);
                    PopulateOtherFields(insuranceUserNotification, parent, timestamp, cost.Id, costStageRevision.Id);
                    await PopulateMetadata(insuranceUserNotification.Object, cost.Id);
                    approver = insuranceUserNotification.Object.Approver;
                    approver.Name = approverName;
                    approver.Type = approvalType;
                    notifications.Add(insuranceUserNotification);
                }

                var financeManagementNotification = await AddFinanceManagerNotification(core.Constants.EmailNotificationActionType.AllApprovalsApproved,
                    cost, costUsers, costStageRevision, timestamp, notifications);
                if (financeManagementNotification != null)
                {
                    approver = financeManagementNotification.Object.Approver;
                    approver.Name = approverName;
                    approver.Type = approvalType;
                }
            }

            return notifications;
        }

        private async Task<string> GetBrandManagerRoleLabel()
        {
            const string brandManagerRoleKey = "Brand Manager";
            return await EFContext.BusinessRole.Where(br => br.Key == brandManagerRoleKey).Select(br => br.Value).SingleAsync();
        }
    }
}

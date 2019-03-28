namespace costs.net.plugins.PG.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core;
    using core.Extensions;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.CustomData;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Form.UsageBuyout;
    using Models;
    using Models.PurchaseOrder;
    using Cost = dataAccess.Entity.Cost;

    internal class RecalledNotificationBuilder : NotificationBuilderBase
    {
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly IApprovalService _approvalService;
        private readonly ICostFormService _costFormService;
        private readonly ICustomObjectDataService _customObjectDataService;

        internal RecalledNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService, IApprovalService approvalService,
            ICostFormService costFormService, ICustomObjectDataService customObjectDataService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext, 
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
            _costStageRevisionService = costStageRevisionService;
            _approvalService = approvalService;
            _costFormService = costFormService;
            _customObjectDataService = customObjectDataService;
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildAsync(Cost cost, CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Cost Owner
            var costOwner = costUsers.CostOwner;
            var actionType = Constants.EmailNotificationActionType.Recalled;
            var costOwnerNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, costOwner.GdamUserId);
            AddSharedTo(costOwnerNotification);
            MapEmailNotificationObject(costOwnerNotification.Object, cost, costOwner);
            await PopulateOtherFieldsForRecall(costOwnerNotification, Constants.EmailNotificationParents.CostOwner, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(costOwnerNotification.Object, cost.Id);
            notifications.Add(costOwnerNotification);

            var approvals = await _approvalService.GetApprovalsByCostStageRevisionId(costStageRevision.Id);

            if (approvals == null)
            {
                //No approvals set
                return notifications;
            }

            var technicalApprovals = approvals.Where(a => a.Type == ApprovalType.IPM).ToArray();
            foreach (var technicalApproval in technicalApprovals)
            {
                foreach (var approvalMember in technicalApproval.ApprovalMembers)
                {
                    //Send notifications to Technical Approver
                    await AddApproverNotification(notifications, cost, costOwner, costStageRevision, timestamp, actionType, approvalMember.CostUser);
                }
            }

            var brandApprovals = approvals.Where(a => a.Type == ApprovalType.Brand).ToArray();
            foreach (var brandApproval in brandApprovals)
            {
                foreach (var approvalMember in brandApproval.ApprovalMembers)
                {
                    //Send notifications to actual Brand Approvers only
                    if (approvalMember.IsSystemApprover())
                    {
                        continue;
                    }

                    await AddApproverNotification(notifications, cost, costOwner, costStageRevision, timestamp, actionType, approvalMember.CostUser);
                }
                if (brandApproval.Requisitioners == null)
                {
                    continue;
                }

                foreach (var requisitioner in brandApproval.Requisitioners)
                {
                    await AddApproverNotification(notifications, cost, costOwner, costStageRevision, timestamp, actionType, requisitioner.CostUser);
                }
            }
            await AddFinanceManagerNotification(actionType, cost, costUsers, costStageRevision, timestamp, notifications);

            return notifications;
        }

        private async Task AddApproverNotification(List<EmailNotificationMessage<CostNotificationObject>> notifications, 
            Cost cost, CostUser costOwner, CostStageRevision costStageRevision, DateTime timestamp, string actionType, CostUser user)
        {
            var notificationMessage = new EmailNotificationMessage<CostNotificationObject>(actionType, user.GdamUserId);
            AddSharedTo(notificationMessage);
            MapEmailNotificationObject(notificationMessage.Object, cost, costOwner, user);
            await PopulateOtherFieldsForRecall(notificationMessage, Constants.EmailNotificationParents.Approver, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(notificationMessage.Object, cost.Id);
            notifications.Add(notificationMessage);
        }

        private async Task PopulateOtherFieldsForRecall(EmailNotificationMessage<CostNotificationObject> message, string parent, DateTime timestamp, Guid costId, Guid costStageRevisionId)
        {
            PopulateOtherFields(message, parent, timestamp, costId, costStageRevisionId);

            // Add fields specific to Recall notification message
            var obj = message.Object;
            var cost = obj.Cost;
            var stageForm = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId).Result;
            var buyoutDetails = _costFormService.GetCostFormDetails<BuyoutDetails>(costStageRevisionId).Result;

            cost.AgencyTrackingNumber = stageForm.AgencyTrackingNumber;
            cost.Region = stageForm.BudgetRegion?.Name;
            cost.AiringCountries = string.Join(";", (buyoutDetails?.AiringCountries ?? new BuyoutDetails.Country[0]).Select(c => c.Name).ToArray());
            cost.Requisition = (await _customObjectDataService.GetCustomData<PgPurchaseOrderResponse>(costStageRevisionId, CustomObjectDataKeys.PgPurchaseOrderResponse))?.Requisition;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using costs.net.core.Helpers;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.dataAccess.Entity;

namespace costs.net.plugins.PG.Builders.Notifications
{
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;

    class PendingTechnicalNotificationBuilder : NotificationBuilderBase
    {
        private readonly IApprovalService _approvalService;

        internal PendingTechnicalNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            IApprovalService approvalService, 
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
            _approvalService = approvalService;
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(dataAccess.Entity.Cost cost,
            CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();
            var approvals = _approvalService.GetApprovalsByCostStageRevisionId(costStageRevision.Id).Result;
            const string actionType = core.Constants.EmailNotificationActionType.TechnicalApproverAssigned;

            if (approvals == null)
            {
                //No approvals set
                return notifications;
            }

            var costOwner = costUsers.CostOwner;
            var technicalApprovals = approvals.Where(a => a.Type == ApprovalType.IPM).ToArray();
            foreach (var technicalApproval in technicalApprovals)
            {
                foreach (var approvalMember in technicalApproval.ApprovalMembers)
                {
                    //Send notifications to Technical Approver
                    var approvalCostUser = approvalMember.CostUser;
                    var technicalApproverNotification = new EmailNotificationMessage<CostNotificationObject>(actionType,
                        approvalCostUser.GdamUserId);
                    AddSharedTo(technicalApproverNotification);

                    MapEmailNotificationObject(technicalApproverNotification.Object, cost, costOwner, approvalCostUser);
                    PopulateOtherFields(technicalApproverNotification, core.Constants.EmailNotificationParents.TechnicalApprover, timestamp, cost.Id, costStageRevision.Id, approvalCostUser);
                    await PopulateMetadata(technicalApproverNotification.Object, cost.Id);
                    notifications.Add(technicalApproverNotification);

                    //Insurance User
                    if (ShouldNotifyInsuranceUsers(costUsers))
                    {
                        var insuranceUserNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, costUsers.InsuranceUsers);
                        AddSharedTo(insuranceUserNotification);

                        MapEmailNotificationObject(insuranceUserNotification.Object, cost, costOwner, approvalCostUser);
                        PopulateOtherFields(insuranceUserNotification, core.Constants.EmailNotificationParents.InsuranceUser, timestamp, cost.Id, costStageRevision.Id,
                            approvalCostUser);
                        await PopulateMetadata(insuranceUserNotification.Object, cost.Id);
                        notifications.Add(insuranceUserNotification);
                    }

                }
            }
            await AddFinanceManagerNotification(actionType, cost, costUsers, costStageRevision, timestamp, notifications);

            return notifications;
        }
    }
}

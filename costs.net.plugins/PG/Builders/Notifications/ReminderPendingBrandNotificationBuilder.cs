using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using costs.net.core.Helpers;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.core.Services.Regions;
using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Extensions;
using Agency = costs.net.dataAccess.Entity.Agency;

namespace costs.net.plugins.PG.Builders.Notifications
{
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using dataAccess;

    class ReminderPendingBrandNotificationBuilder : NotificationBuilderBase
    {
        private readonly IApprovalService _approvalService;
        private readonly IRegionsService _regionService;

        internal ReminderPendingBrandNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService, IApprovalService approvalService,
            IMetadataProviderService metadataProviderService,
            IRegionsService regionsService,
            AppSettings appSettings,
            EFContext efContext) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
            _approvalService = approvalService;
            _regionService = regionsService;
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(dataAccess.Entity.Cost cost,
            CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Build PendingBrandApproval notifications
            var approvals = _approvalService.GetApprovalsByCostStageRevisionId(costStageRevision.Id).Result;
            if (approvals != null)
            {
                var costOwner = costUsers.CostOwner;
                await BuildPendingBrandApprovalNotification(notifications, approvals, cost, costOwner, costStageRevision.Id, timestamp);
            }

            return notifications;
        }

        private async Task BuildPendingBrandApprovalNotification(List<EmailNotificationMessage<CostNotificationObject>> notifications,
            List<Approval> approvals, dataAccess.Entity.Cost cost, CostUser costOwner, Guid costStageRevisionId, DateTime timestamp)
        {
            var brandApprovals = approvals.Where(a => a.Type == ApprovalType.Brand).ToArray();
            bool isCyclone = IsNorthAmericanCycloneAgency(costOwner.Agency);
            string actionType = core.Constants.EmailNotificationActionType.BrandApproverSendReminder;
            foreach (var brandApproval in brandApprovals)
            {
                if (isCyclone)
                {
                    var parent = core.Constants.EmailNotificationParents.BrandApprover;
                    //Send notification to Brand Approver in the Platform for North American Cyclone agencies to every approval member
                    foreach (var approvalMember in brandApproval.ApprovalMembers)
                    {
                        var brandApproverNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, approvalMember.CostUser.GdamUserId);
                        AddSharedTo(brandApproverNotification);

                        MapEmailNotificationObject(brandApproverNotification.Object, cost, costOwner, approvalMember.CostUser);
                        PopulateOtherFields(brandApproverNotification, parent, timestamp, cost.Id, costStageRevisionId);
                        await PopulateMetadata(brandApproverNotification.Object, cost.Id);
                        notifications.Add(brandApproverNotification);
                    }
                }
                else
                {
                    //Send one email to Cost Owner for approval in Coupa
                    var parent = core.Constants.EmailNotificationParents.Coupa;
                    var brandApproverNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, costOwner.GdamUserId);
                    AddSharedTo(brandApproverNotification);

                    MapEmailNotificationObject(brandApproverNotification.Object, cost, costOwner, costOwner);
                    PopulateOtherFields(brandApproverNotification, parent, timestamp, cost.Id, costStageRevisionId);
                    await PopulateMetadata(brandApproverNotification.Object, cost.Id);
                    notifications.Add(brandApproverNotification);
                }
            }
        }

        private bool IsNorthAmericanCycloneAgency(Agency agency)
        {
            if (!agency.IsCyclone())
            {
                return false;
            }

            if (!agency.IsNorthAmericanAgency(_regionService))
            {
                return false;
            }

            return true;
        }
    }
}

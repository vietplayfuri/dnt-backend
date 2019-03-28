using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using costs.net.core.Helpers;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Extensions;
using Serilog;

namespace costs.net.plugins.PG.Builders.Notifications
{
    using System.Threading.Tasks;
    using core.Extensions;
    using core.Models.Utils;
    using core.Services.CustomData;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using Models;
    using Services;
    using Cost = dataAccess.Entity.Cost;

    class PendingBrandNotificationBuilder : NotificationBuilderBase
    {
        private static readonly ILogger Logger = Log.ForContext<PendingBrandNotificationBuilder>();

        private readonly IApprovalService _approvalService;
        private readonly ICustomObjectDataService _customObjectDataService;
        private readonly IPgPaymentService _pgPaymentService;
        private readonly ICostStageService _costStageService;

        internal PendingBrandNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService, IApprovalService approvalService,
            ICustomObjectDataService customObjectDataService,
            IPgPaymentService pgPaymentService,
            ICostStageService costStageService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext, 
            IRegionsService regionsService) :
            base(mapper, uriHelper, costStageRevisionService, metadataProviderService, appSettings, efContext, regionsService)
        {
            _approvalService = approvalService;
            _customObjectDataService = customObjectDataService;
            _pgPaymentService = pgPaymentService;
            _costStageService = costStageService;
        }

        internal async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> Build(Cost cost,
            CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();

            //Build PendingBrandApproval notifications
            var approvals = _approvalService.GetApprovalsByCostStageRevisionId(costStageRevision.Id).Result;
            if (approvals != null)
            {
                await BuildPendingBrandApprovalNotification(notifications, approvals, cost, costUsers, costStageRevision, timestamp);
            }

            return notifications;
        }

        private async Task BuildPendingBrandApprovalNotification(List<EmailNotificationMessage<CostNotificationObject>> notifications,
            List<Approval> approvals, Cost cost, CostNotificationUsers costUsers, CostStageRevision costStageRevision, DateTime timestamp)
        {
            var costOwner = costUsers.CostOwner;
            var agency = costOwner.Agency;
            var isCyclone = agency.IsCyclone();
            var costStageRevisionId = costStageRevision.Id;
            var isNorthAmericanBudgetRegion = IsNorthAmericanBudgetRegion(costStageRevisionId);

            var brandApprovals = approvals.Where(a => a.Type == ApprovalType.Brand).ToArray();

            if (isCyclone && isNorthAmericanBudgetRegion)
            {
                //Send notification to Brand Approver in the Platform for Cost in North American Budget Region and Cyclone agencies.
                const string actionType = core.Constants.EmailNotificationActionType.BrandApproverAssigned;
                var parent = core.Constants.EmailNotificationParents.BrandApprover;
                foreach (var brandApproval in brandApprovals)
                {
                    foreach (var approvalMember in brandApproval.ApprovalMembers)
                    {
                        //Send notifications to actual Brand Approvers only
                        if (approvalMember.IsSystemApprover())
                        {
                            continue;
                        }

                        var approvalCostUser = approvalMember.CostUser;
                        var brandApproverNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, approvalCostUser.GdamUserId);
                        AddSharedTo(brandApproverNotification);

                        MapEmailNotificationObject(brandApproverNotification.Object, cost, costOwner, approvalCostUser);
                        PopulateOtherFields(brandApproverNotification, parent, timestamp, cost.Id, costStageRevisionId);
                        await PopulateMetadata(brandApproverNotification.Object, cost.Id);
                        notifications.Add(brandApproverNotification);

                        if (ShouldNotifyInsuranceUsers(costUsers))
                        {
                            var insuranceUserNotification = new EmailNotificationMessage<CostNotificationObject>(actionType, costUsers.InsuranceUsers);
                            AddSharedTo(insuranceUserNotification);

                            MapEmailNotificationObject(insuranceUserNotification.Object, cost, costOwner, approvalCostUser);
                            PopulateOtherFields(insuranceUserNotification, core.Constants.EmailNotificationParents.InsuranceUser, timestamp, cost.Id, costStageRevisionId);
                            await PopulateMetadata(insuranceUserNotification.Object, cost.Id);
                            notifications.Add(insuranceUserNotification);
                        }
                    }
                }

                if (brandApprovals.Any())
                {
                    await AddFinanceManagerNotification(actionType, cost, costUsers, costStageRevision, timestamp, notifications);
                }
            }
            else
            {
                AddCoupaApprovalEmail(notifications, cost, costOwner, costStageRevisionId, timestamp);
            }
        }

        // TODO cover this logc by unit tests
        private void AddCoupaApprovalEmail(List<EmailNotificationMessage<CostNotificationObject>> notifications, 
            Cost cost, CostUser costOwner, Guid costStageRevisionId, DateTime timestamp)
        {
            var previousCostStage = _costStageService.GetPreviousCostStage(cost.LatestCostStageRevision.CostStageId).Result;

            if (previousCostStage == null)
            {
                // No need to send COUPA apprvoal email because this is the first time cost gets submitted for Brand Approval
                return;
            }

            var latestRevisionOfPreviousStage = CostStageRevisionService.GetLatestRevision(previousCostStage.Id).Result;
            if (latestRevisionOfPreviousStage == null)
            {
                throw new Exception($"Couldn't find latest revision for stage {previousCostStage.Id}");
            }

            var previousPaymentAmount = _pgPaymentService.GetPaymentAmount(latestRevisionOfPreviousStage.Id, false).Result;
            var currentPaymentAmount = _pgPaymentService.GetPaymentAmount(costStageRevisionId, false).Result;

            if (currentPaymentAmount.TotalCostAmount == previousPaymentAmount.TotalCostAmount)
            {
                return;
            }

            // Send COUPA approval email because total amount changed 
            var paymentDetails = _customObjectDataService.GetCustomData<PgPaymentDetails>(costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails).Result;
            if (!string.IsNullOrEmpty(paymentDetails?.PoNumber))
            {
                var actionType = core.Constants.EmailNotificationActionType.Submitted;
                var parent = core.Constants.EmailNotificationParents.Coupa;
                var coupaNotification = new EmailNotificationMessage<CostNotificationObject>(actionType);

                MapEmailNotificationObject(coupaNotification.Object, cost, costOwner);
                PopulateOtherFields(coupaNotification, parent, timestamp, cost.Id, cost.LatestCostStageRevision.Id);
                AddSharedTo(coupaNotification);
                var notificationCost = coupaNotification.Object.Cost;
                notificationCost.PurchaseOrder = new PurchaseOrder();
                Mapper.Map(currentPaymentAmount, notificationCost.PurchaseOrder);
                Mapper.Map(paymentDetails, notificationCost.PurchaseOrder);
                coupaNotification.Parameters.EmailService.AdditionalEmails.Add(AppSettings.CoupaApprovalEmail);

                notifications.Add(coupaNotification);
            }
        }
    }
}

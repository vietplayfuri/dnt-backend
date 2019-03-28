using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using costs.net.core.Analysis;
using costs.net.core.Builders.Notifications;
using costs.net.core.ExternalResource.Paperpusher;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Costs;
using costs.net.core.Services.Notifications;
using costs.net.dataAccess;
using costs.net.dataAccess.Entity;
using Microsoft.EntityFrameworkCore;

namespace costs.net.plugins.PG.Services.Notifications
{
    using core.Extensions;
    using core.Models.ACL;
    using Models.Stage;
    using Cost = dataAccess.Entity.Cost;

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly EFContext _efContext;
        private readonly IEmailNotificationBuilder _emailNotificationBuilder;
        private readonly IPaperpusherClient _paperPusherClient;
        private readonly IEmailNotificationReminderService _reminderService;
        private readonly ICostUserService _costUserService;
        private readonly IApprovalService _approvalService;

        public EmailNotificationService(IEmailNotificationBuilder emailNotificationBuilder, IPaperpusherClient paperPusherClient, 
            IEmailNotificationReminderService reminderService, 
            EFContext efContext,
            ICostUserService costUserService,
            IApprovalService approvalService)
        {
            _emailNotificationBuilder = emailNotificationBuilder;
            _paperPusherClient = paperPusherClient;
            _reminderService = reminderService;
            _efContext = efContext;
            _costUserService = costUserService;
            _approvalService = approvalService;
        }

        public async Task<bool> CostHasBeenSubmitted(Guid costId, Guid userId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr => csr.CostStage)
                        .ThenInclude(cs => cs.CostStageRevisions)
                .Include(c => c.LatestCostStageRevision)                    
                .Include(c => c.CostStages)
                    .ThenInclude(cs => cs.CostStageRevisions)
                        .ThenInclude(csr => csr.Approvals)
                            .ThenInclude(a => a.ApprovalMembers)
                                .ThenInclude(am => am.CostUser)
                .Include(c => c.Project)
                    .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var latestRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = cost.Owner,
                InsuranceUsers = insuranceUsers,
                FinanceManagementUsers = financeManagementUsers
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostSubmittedNotification(costUsers, cost, latestRevision, DateTime.UtcNow);
            var notifications = notificationMessages.ToList();

            //Upon Submit the next stage might be PendingTechnicalApproval or PendingBrandApproval or Approved for FA.
            var nextStatus = cost.Status;
            switch(nextStatus)
            {
                case CostStageRevisionStatus.Approved:
                    // The Cost is auto approved when the budget does not change between cost stages.
                    await BuildAutoApprovedNotification(costUsers, cost, latestRevision, notifications);
                    break;
                case CostStageRevisionStatus.PendingBrandApproval:
                    var pendingBrandNotifications = await _emailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost, latestRevision, DateTime.UtcNow);
                    notifications.AddRange(pendingBrandNotifications);

                    //Enqueue reminder for the brand approvers.
                    await _reminderService.CreateNew(cost.Id, DateTime.UtcNow.AddDays(1));
                    break;

                case CostStageRevisionStatus.PendingTechnicalApproval:
                    var pendingTechnicalNotifications = await _emailNotificationBuilder.BuildPendingTechnicalApprovalNotification(costUsers, cost, latestRevision, DateTime.UtcNow);
                    notifications.AddRange(pendingTechnicalNotifications);
                    break;

                    //ADC-2698 comment this line of code to take off the feature from release 1.9.4
                    //    //Enqueue reminder for the technical approvers.
                    //    await _reminderService.CreateNew(cost.Id, DateTime.UtcNow.AddDays(2));
                    //    break;
            }

            //Notify previous approvers if they've been removed
            if (cost.HasPreviousRevision())
            {
                var previousRevision = cost.GetPreviousRevision();
                var previousRevisionId = previousRevision.Id;
                //Get the approvers for the previous revision
                previousRevision.Approvals = await _approvalService.GetApprovalsByCostStageRevisionId(previousRevisionId);

                var csrAnalyser = new CostStageRevisionAnalyser();
                IEnumerable<ApprovalMember> removedApprovers = csrAnalyser.GetRemovedApprovers(previousRevision, latestRevision);

                var removedApproverNotifications = await _emailNotificationBuilder.BuildPreviousApproverNotification(costUsers, 
                    removedApprovers, cost, previousRevision, DateTime.UtcNow);
                notifications.AddRange(removedApproverNotifications);
            }

            bool sent = false;
            foreach (var message in notifications)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            return sent;
        }

        private async Task BuildAutoApprovedNotification(CostNotificationUsers costUsers, Cost cost, CostStageRevision latestRevision, List<EmailNotificationMessage<CostNotificationObject>> notifications)
        {
            var approver = await GetLastApprover(cost);
            notifications.AddRange(await _emailNotificationBuilder.BuildCostApprovedNotification(costUsers,
                cost, latestRevision, approver, core.Constants.EmailApprovalType.Brand, DateTime.UtcNow));
        }

        private async Task<string> GetLastApprover(Cost cost)
        {
            Guid? approverUserId = null;
            if (!cost.IsExternalPurchases)
            {
                //Cyclone, Get the last approver from the previous stages
                var costStages = cost.CostStages.OrderBy(cs => cs.Modified).ToArray();
                var position = costStages.Length;
                while (approverUserId == null || position == -1)
                {
                    position--;
                    var previousStage = costStages[position];
                    var lastRevision = previousStage.CostStageRevisions.OrderBy(csr => csr.Modified).Last();
                    approverUserId = lastRevision
                        .Approvals?
                        .Where(a => a.Type == ApprovalType.Brand)
                        .SingleOrDefault()?
                        .ApprovalMembers?.Select(am => am.MemberId).FirstOrDefault();
                }
            }
            else
            {
                // Coupa Requisitioner so no 'Approver'
                approverUserId = Guid.Empty;
            }
            // Coupa Requisitioner so no 'Approver'
            return await _costUserService.GetApprover(cost.Id, approverUserId.Value, core.Constants.EmailApprovalType.Brand);
        }

        public async Task<bool> CostHasBeenApproved(Guid costId, Guid approverUserId, string approvalType)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (approverUserId == Guid.Empty)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(approvalType))
            {
                return false;
            }

            if (!IsValidApprovalType(approvalType))
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;
            var approver = await _costUserService.GetApprover(costId, approverUserId, approvalType);
            var costOwner = cost.Owner;
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                InsuranceUsers = insuranceUsers,
                Watchers = await GetWatchers(costId)
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost, costStageRevision, approver, approvalType, DateTime.UtcNow);
            var notifications = notificationMessages.ToList();

            if (approvalType != core.Constants.EmailApprovalType.Brand)
            {
                //Technical Approval sent so now send Brand Approval email
                notifications.AddRange(await _emailNotificationBuilder.BuildPendingBrandApprovalNotification(costUsers, cost, costStageRevision, DateTime.UtcNow));

                //For Technical Approval, cancel the reminder, if any
                await _reminderService.CancelReminder(cost.Id);

                //Enqueue reminder for the brand approvers.
                await _reminderService.CreateNew(cost.Id, DateTime.UtcNow.AddDays(1));
            }
            if (approvalType == core.Constants.EmailApprovalType.Brand)
            {
                //For Brand Approval, cancel the reminder, if any
                await _reminderService.CancelReminder(cost.Id);
            }

            bool sent = false;
            foreach (var message in notifications)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            return sent;
        }

        public async Task<bool> CostHasBeenRecalled(Guid costId, Guid userId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;
            var recaller = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Id == userId);
            var costOwner = cost.Owner;
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = cost.Owner,
                InsuranceUsers = insuranceUsers,
                FinanceManagementUsers = financeManagementUsers
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostRecalledNotification(
                costUsers, 
                cost, 
                costStageRevision, 
                recaller, 
                DateTime.UtcNow);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            //For Brand Approval, cancel the reminder, if any
            await _reminderService.CancelReminder(cost.Id);

            return sent;
        }

        public async Task<bool> CostHasBeenRequestedToBeReopened(Guid costId, Guid userId, Guid userModuleId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;

            var adminUsers = await _efContext.CostUser.Include(x => x.UserUserGroups).ThenInclude(x => x.UserGroup)
                .Where(u => u.UserUserGroups.Any(x => x.UserGroup.Role.Name == Roles.ClientAdmin && x.UserGroup.ObjectId == userModuleId)).ToListAsync();
            var requestedBy = await _efContext.CostUser.FindAsync(userId);

            var costOwner = cost.Owner;
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                FinanceManagementUsers = financeManagementUsers,
                InsuranceUsers = insuranceUsers,
                Watchers = await GetWatchers(costId),
                ClientAdmins = adminUsers
            };

            var notificationMessages = await _emailNotificationBuilder.BuildCostReopenRequestedNotification(
                costUsers,
                cost,
                costStageRevision,
                DateTime.UtcNow,
                requestedBy);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            //For Brand Approval, cancel the reminder, if any
            await _reminderService.CancelReminder(cost.Id);

            return sent;
        }

        public async Task<bool> CostReopenApproved(Guid costId, Guid userId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = cost.Owner,
                InsuranceUsers = insuranceUsers
            };

            var approvedBy = await _efContext.CostUser.FindAsync(userId);

            var notificationMessages = await _emailNotificationBuilder.BuildCostReopenApprovedNotification(
                costUsers,
                cost,
                costStageRevision,
                DateTime.UtcNow,
                approvedBy);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            //For Brand Approval, cancel the reminder, if any
            await _reminderService.CancelReminder(cost.Id);

            return sent;
        }

        public async Task<bool> CostReopenRejected(Guid costId, Guid userId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = cost.Owner,
                InsuranceUsers = insuranceUsers
            };
            var rejectedBy = await _efContext.CostUser.FindAsync(userId);

            var notificationMessages = await _emailNotificationBuilder.BuildCostReopenRejectedNotification(
                costUsers,
                cost,
                costStageRevision,
                DateTime.UtcNow,
                rejectedBy);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            //For Brand Approval, cancel the reminder, if any
            await _reminderService.CancelReminder(cost.Id);

            return sent;
        }

        public async Task<bool> CostHasBeenRejected(Guid costId, Guid userId, string approvalType, string comments)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(approvalType))
            {
                return false;
            }

            if (!IsValidApprovalType(approvalType))
            {
                return false;
            }

            if (comments == null)
            {
                comments = string.Empty;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var costStageRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var rejecter = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Id == userId);
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = insuranceUsers,
                Watchers = await GetWatchers(costId),
                FinanceManagementUsers = financeManagementUsers
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, costStageRevision, rejecter, approvalType, comments, DateTime.UtcNow);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            //For Brand Approval, cancel the reminder, if any
            await _reminderService.CancelReminder(cost.Id);

            return sent;
        }

        public async Task<bool> CostHasBeenCancelled(Guid costId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (cost.Status != CostStageRevisionStatus.Cancelled)
            {
                // Cost.Status might be PendingCancellation so don't send an email until
                // the Cancellation message has been sent from Coupa to Costs via AMQ.
                return false;
            }

            var costStageRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var financeManagementUsers = await _costUserService.GetFinanceManagementUsers(cost.UserGroups, Constants.BudgetRegion.NorthAmerica);
            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = insuranceUsers,
                Watchers = await GetWatchers(costId),
                FinanceManagementUsers = financeManagementUsers
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, costStageRevision, DateTime.UtcNow);
            bool sent = false;
            foreach (var message in notificationMessages)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            return sent;
        }
        
        public async Task<bool> CostOwnerHasChanged(Guid costId, Guid userId, Guid previousOwnerId)
        {
            if (costId == Guid.Empty)
            {
                return false;
            }

            if (userId == Guid.Empty)
            {
                return false;
            }

            if (previousOwnerId == Guid.Empty)
            {
                return false;
            }

            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(csr => csr.CostStage)
                .ThenInclude(cs => cs.CostStageRevisions)
                .Include(c => c.Project)
                .ThenInclude(p => p.Brand)
                .IncludeCostOwner()
                .FirstOrDefaultAsync(c => c.Id == costId);
            var latestRevision = cost.LatestCostStageRevision;
            var costOwner = cost.Owner;
            var previousOwner = await _efContext.CostUser.FirstOrDefaultAsync(x => x.Id == previousOwnerId);
            var changeApprover = await _efContext.CostUser.FirstOrDefaultAsync(x => x.Id == userId);
            var insuranceUsers = await _costUserService.GetInsuranceUsers(costOwner.Agency);
            var watchers = await GetWatchers(costId);
            var approvals = _approvalService.GetApprovalsByCostStageRevisionId(latestRevision.Id).Result;
            var approvers = approvals?.SelectMany(a => a.ApprovalMembers
                                .Where(m => m.CostUser.Email != core.Builders.Response.ApprovalMemberModel.BrandApprovalUserEmail)
                                .Select(m => m.CostUser.GdamUserId))
                            ?? new List<string>();

            var costUsers = new CostNotificationUsers
            {
                CostOwner = cost.Owner,
                InsuranceUsers = insuranceUsers,
                Watchers = watchers,
                Approvers = approvers,
            };
            var notificationMessages = await _emailNotificationBuilder.BuildCostOwnerChangedNotification(costUsers, cost, latestRevision, DateTime.UtcNow, changeApprover, previousOwner);
            var notifications = notificationMessages.ToList();

            bool sent = false;
            foreach (var message in notifications)
            {
                sent = await _paperPusherClient.SendMessage(message);
            }

            return sent;
        }

        private static bool IsValidApprovalType(string approvalType)
        {
            ApprovalType unused;
            return Enum.TryParse(approvalType, out unused);
        }

        private async Task<List<string>> GetWatchers(Guid costId)
        {
            return await _efContext.NotificationSubscriber
                .Include(ns => ns.CostUser)
                .Where(a => a.CostId == costId && a.Owner == false)
                .Select(ns => ns.CostUser.GdamUserId).ToListAsync();
        }

        
    }
}

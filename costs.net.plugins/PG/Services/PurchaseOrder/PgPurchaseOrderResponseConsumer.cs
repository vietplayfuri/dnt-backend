namespace costs.net.plugins.PG.Services.PurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.Response;
    using core.Exceptions;
    using core.Messaging.Messages;
    using core.Models;
    using core.Models.ActivityLog;
    using core.Models.Response;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.CustomData;
    using core.Services.Events;
    using core.Services.Notifications;
    using core.Services.PurchaseOrder;
    using core.Services.Workflow;
    using dataAccess;
    using Models.PurchaseOrder;
    using Newtonsoft.Json;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class PgPurchaseOrderResponseConsumer : IPurchaseOrderResponseConsumer
    {
        private readonly EFContext _efContext;
        private readonly ICustomObjectDataService _customDataService;
        private readonly IMapper _mapper;        
        private readonly IApprovalService _approvalService;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly ICostActionService _costActionService;
        private readonly IEventService _eventService;
        private readonly IPgPaymentService _pgPaymentService;
        private readonly IActivityLogService _activityLogService;

        public PgPurchaseOrderResponseConsumer(EFContext efContext,
            ICustomObjectDataService customDataService,
            IMapper mapper,           
            IApprovalService approvalService,
            IEmailNotificationService emailNotificationService,
            ICostActionService costActionService,
            IEventService eventService,
            IPgPaymentService pgPaymentService,
            IActivityLogService activityLogService)
        {
            _efContext = efContext;
            _customDataService = customDataService;
            _mapper = mapper;            
            _approvalService = approvalService;
            _emailNotificationService = emailNotificationService;
            _costActionService = costActionService;
            _eventService = eventService;
            _pgPaymentService = pgPaymentService;
            _activityLogService = activityLogService;
        }

        public async Task Consume(PurchaseOrderResponse message)
        {
            var cost = await _efContext.Cost
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CostNumber == message.CostNumber);
            if (cost == null)
            {
                throw new XmgException($"Could't find the cost with Cost Number {message.CostNumber}") {ClientName = message.ClientName};
            }

            try
            {
                var payload = JsonConvert.DeserializeObject<PgPurchaseOrderResponse>(message.Payload.ToString());

                var adminUser = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Email == ApprovalMemberModel.BrandApprovalUserEmail);
                var adminUserIdentity = new SystemAdminUserIdentity(adminUser);
                await _efContext.InTransactionAsync(async () =>
                {
                    await SaveResponse(payload, cost, adminUserIdentity);

                    await ActionOnCost(message, payload, cost, adminUserIdentity);

                    if (payload.ApprovalStatus == ApprovalStatuses.Approved ||
                        // ADC-1731
                        (payload.ApprovalStatus == ApprovalStatuses.AwaitingDecisionInCost &&
                            ApprovalRequired(cost) && await HasCostBeenApproved(cost, payload)))
                    {
                        // payload details are required in the email so the response needs to be saved before we can send the email
                        await _emailNotificationService.CostHasBeenApproved(cost.Id, adminUserIdentity.Id, ApprovalType.Brand.ToString());
                    }
                }, 
                () => _eventService.SendAllPendingAsync());
            }
            catch (Exception ex)
            {
                var messageJson = JsonConvert.SerializeObject(message);
                throw new XmgException($"Failed to consume response message: {messageJson} from Coupa {ex}", ex);
            }
        }

        private async Task ActionOnCost(PurchaseOrderResponse message, PgPurchaseOrderResponse payload, Cost cost, SystemAdminUserIdentity adminUser)
        {
            var adminUserId = adminUser.Id;

            OperationResponse response;
            switch (message.ActivityType)
            {
                case ActivityTypes.Updated:
                    switch (payload.ApprovalStatus)
                    {
                        case ApprovalStatuses.Approved:
                            if (ApprovalRequired(cost))
                            {
                                await ApproveCost(cost.Id, adminUser);
                            }
                            break;
                        case ApprovalStatuses.Rejected:
                            message.Payload.TryGetValue(nameof(PgPurchaseOrderResponse.Comments), StringComparison.OrdinalIgnoreCase, out var commentsToken);
                            var comments = commentsToken != null ?commentsToken.ToObject<string>() : string.Empty;
                            response = await _approvalService.Reject(cost.Id, adminUser, BuType.Pg, comments, SourceSystem.Coupa);
                            if (response.Success)
                            {
                                await _emailNotificationService.CostHasBeenRejected(cost.Id, adminUserId, ApprovalType.Brand.ToString(), comments);
                            }

                            break;
                        // ADC-1731 Dealing with COUPA's limitations
                        case ApprovalStatuses.AwaitingDecisionInCost:
                            if (ApprovalRequired(cost) && await HasCostBeenApproved(cost, payload))
                            {
                                await ApproveCost(cost.Id, adminUser);
                            }
                            break;
                    }
                    break;
                case ActivityTypes.Cancelled:
                    response = await _costActionService.CompleteCancel(cost.Id, BuType.Pg);
                    if (response.Success)
                    {
                        await _emailNotificationService.CostHasBeenCancelled(cost.Id);
                    }

                    break;
                case ActivityTypes.Recalled:
                    await _costActionService.CompleteRecall(cost.Id, adminUser);
                    break;
            }
        }

        private async Task ApproveCost(Guid costId, UserIdentity userIdentity)
        {
            await _approvalService.Approve(costId, userIdentity, BuType.Pg, SourceSystem.Coupa);
        }

        private async Task SaveResponse(PgPurchaseOrderResponse payload, Cost cost, SystemAdminUserIdentity adminUser)
        {
            var purchaseOrderData = await _customDataService
                .GetCustomData<PgPurchaseOrderResponse>(cost.LatestCostStageRevisionId.Value, CustomObjectDataKeys.PgPurchaseOrderResponse)
                ?? new PgPurchaseOrderResponse();
            _mapper.Map(payload, purchaseOrderData);
            if (string.Compare(payload.ApprovalStatus, ApprovalStatuses.Rejected, StringComparison.OrdinalIgnoreCase) == 0)
            {
                //null requisition ID should be allowed here
                purchaseOrderData.Requisition = payload.Requisition;
                purchaseOrderData.ApprovalStatus = payload.ApprovalStatus;
            }

            await _customDataService.Save(cost.LatestCostStageRevisionId.Value, CustomObjectDataKeys.PgPurchaseOrderResponse, purchaseOrderData, adminUser);

            await UpdatePaymentDetails(cost.LatestCostStageRevisionId.Value, purchaseOrderData, adminUser);

            var logEntries = new List<IActivityLogEntry>
            {
                new PoCreated(cost.CostNumber, purchaseOrderData.PoNumber, adminUser),
                new GoodsReceiptAllocated(cost.CostNumber, purchaseOrderData.GrNumber, adminUser),
                new RequisitionNumber(cost.CostNumber, purchaseOrderData.Requisition, adminUser)
            };

            await _activityLogService.LogRange(logEntries);
        }

        private async Task<PgPaymentDetails> UpdatePaymentDetails(
            Guid costStageRevisionId, 
            PgPurchaseOrderResponse purchaseOrderData, 
            SystemAdminUserIdentity adminUser)
        {
            var paymentDetails = await _customDataService
                .GetCustomData<PgPaymentDetails>(costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails)
                ?? new PgPaymentDetails();
            _mapper.Map(purchaseOrderData, paymentDetails);
            if ( string.Compare(purchaseOrderData.ApprovalStatus, ApprovalStatuses.Rejected, StringComparison.OrdinalIgnoreCase) == 0)
            {
                //null requisition ID should be allowed here
                paymentDetails.Requisition = purchaseOrderData.Requisition;
            }

            await _customDataService.Save(costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails, paymentDetails, adminUser);

            return paymentDetails;
        }

        private async Task<bool> HasCostBeenApproved(Cost cost, PgPurchaseOrderResponse payload)
        {
            // If TotalAmount in incoming messaage is the same as current cost total amount consider const as "Approved"
            var costStageRevisionId = cost.LatestCostStageRevisionId.Value;
            
            var totalAmountInApplicableCurrency = 0m;
            var paymentAmount = await _pgPaymentService.GetPaymentAmount(costStageRevisionId, false);
            if (paymentAmount != null)
            {
                var rateMultiplier = cost.ExchangeRate ?? 1m;
                
                totalAmountInApplicableCurrency = (paymentAmount.TotalCostAmount ?? 0) / rateMultiplier;
            }
            return payload.TotalAmount.HasValue 
                && Math.Round(totalAmountInApplicableCurrency, 2) == Math.Round(payload.TotalAmount.Value, 2);
        }

        private bool ApprovalRequired(Cost cost)
        {
            return cost.Status == CostStageRevisionStatus.PendingBrandApproval;
        }
    }
}

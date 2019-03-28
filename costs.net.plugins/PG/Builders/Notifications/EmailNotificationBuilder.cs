namespace costs.net.plugins.PG.Builders.Notifications
{
    using AutoMapper;
    using core.Builders.Notifications;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.Regions;
    using dataAccess.Entity;
    using Microsoft.Extensions.Options;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.CustomData;
    using core.Services.Notifications;
    using dataAccess;
    using Services;
    using Cost = dataAccess.Entity.Cost;

    public class EmailNotificationBuilder : IEmailNotificationBuilder
    {
        private static readonly ILogger Logger = Log.ForContext<EmailNotificationBuilder>();

        private readonly IApprovalService _approvalService;
        private readonly AppSettings _appSettings;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly IMapper _mapper;
        private readonly IApplicationUriHelper _uriHelper;
        private readonly IRegionsService _regionsService;
        private readonly ICostFormService _costFormService;
        private readonly ICustomObjectDataService _customObjectDataService;
        private readonly IPgPaymentService _pgPaymentService;
        private readonly ICostStageService _costStageService;
        private readonly IMetadataProviderService _metadataProviderService;
        private readonly EFContext _efContext;

        public EmailNotificationBuilder(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IApprovalService approvalService, IRegionsService regionsService,
            IOptions<AppSettings> appSettings, ICostFormService costFormService,
            ICustomObjectDataService customObjectDataService,
            IPgPaymentService pgPaymentService,
            ICostStageService costStageService,
            IMetadataProviderService metadataProviderService,
            EFContext efContext)
        {
            _mapper = mapper;
            _uriHelper = uriHelper;
            _costStageRevisionService = costStageRevisionService;
            _approvalService = approvalService;
            _regionsService = regionsService;
            _costFormService = costFormService;
            _customObjectDataService = customObjectDataService;
            _pgPaymentService = pgPaymentService;
            _costStageService = costStageService;
            _metadataProviderService = metadataProviderService;
            _efContext = efContext;
            _appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostSubmittedNotification(CostNotificationUsers costUsers,
            Cost cost, CostStageRevision costStageRevision, DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var submittedNotificationBuilder = new SubmittedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await submittedNotificationBuilder.Build(cost, costUsers, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildPendingTechnicalApprovalNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision, DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var pendingTechnicalNotificationBuilder = new PendingTechnicalNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _approvalService, _appSettings, _efContext, _regionsService);
            return await pendingTechnicalNotificationBuilder.Build(cost, costUsers, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildPendingBrandApprovalNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var pendingBrandNotificationBuilder = new PendingBrandNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _approvalService, _customObjectDataService,
                _pgPaymentService, _costStageService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await pendingBrandNotificationBuilder.Build(cost, costUsers, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostApprovedNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            string approverName,
            string approvalType,
            DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (approverName == null)
            {
                Logger.Error("ENB001: Failed to create an email notification because approver is null. This could be because IoNumberOwner is empty from Coupa. The IoNumberOwner is the approver for Costs approved in Coupa.");
                throw new ArgumentNullException(nameof(approverName));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notifications = new List<EmailNotificationMessage<CostNotificationObject>>();
            var approvedNotificationBuilder = new ApprovedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            notifications.AddRange(await approvedNotificationBuilder.Build(cost, costUsers, approverName, approvalType, costStageRevision, timestamp));

            return notifications;
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostRecalledNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            CostUser recaller,
            DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new RecalledNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _approvalService,
                _costFormService, _customObjectDataService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await notificationBuilder.BuildAsync(cost, costUsers, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostRejectedNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            CostUser rejecter,
            string approvalType,
            string comments,
            DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because recipients is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new RejectedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _regionsService, _appSettings, _efContext);
            return await notificationBuilder.Build(cost, costUsers, rejecter.FullName, approvalType, comments, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostCancelledNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new CancelledNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _regionsService, _appSettings, _efContext);
            return await notificationBuilder.Build(cost, costUsers, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostReminderNotification(CostNotificationUsers costUsers,
            Cost cost, CostStageRevision costStageRevision, DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            //ADC-2698 send notification to technical approver
            var notificationMessages = new List<EmailNotificationMessage<CostNotificationObject>>();
            if (costStageRevision.Status == CostStageRevisionStatus.PendingTechnicalApproval)
            {
                var pendingTechnicalNotificationBuilder = new ReminderPendingTechnicalNotificationBuilder(_mapper,
               _uriHelper, _costStageRevisionService, _metadataProviderService, _approvalService, _appSettings, _efContext, _regionsService);
                notificationMessages.AddRange(await pendingTechnicalNotificationBuilder.Build(cost, costUsers, costStageRevision, timestamp));
            }
            else if (costStageRevision.Status == CostStageRevisionStatus.PendingBrandApproval)
            {
                var pendingBrandNotificationBuilder = new ReminderPendingBrandNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _approvalService, _metadataProviderService, _regionsService, _appSettings, _efContext);
                notificationMessages.AddRange(await pendingBrandNotificationBuilder.Build(cost, costUsers, costStageRevision, timestamp));
            }

            return notificationMessages;
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildPreviousApproverNotification(CostNotificationUsers costUsers,
            IEnumerable<ApprovalMember> removedApprovers,
            Cost cost, CostStageRevision previousRevision, DateTime timestamp)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (previousRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(previousRevision));
            }

            if (removedApprovers == null)
            {
                Logger.Error("Failed to create an email notification because removedApprovers is null.");
                throw new ArgumentNullException(nameof(removedApprovers));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var previousApproverNotificationBuilder = new PreviousApproverNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await previousApproverNotificationBuilder.Build(removedApprovers, cost, costUsers, previousRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostReopenApprovedNotification(CostNotificationUsers costUsers, Cost cost, CostStageRevision costStageRevision, DateTime timestamp, CostUser approvedBy)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (approvedBy == null)
            {
                Logger.Error("Failed to create an email notification because approvedBy is null.");
                throw new ArgumentNullException(nameof(approvedBy));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new ReopenApprovedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await notificationBuilder.Build(cost, costUsers, approvedBy.FullName, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostReopenRejectedNotification(CostNotificationUsers costUsers, Cost cost, CostStageRevision costStageRevision, DateTime timestamp, CostUser rejectedBy)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers == null)
            {
                Logger.Error("Failed to create an email notification because costUsers is null.");
                throw new ArgumentNullException(nameof(costUsers));
            }

            if (rejectedBy == null)
            {
                Logger.Error("Failed to create an email notification because rejectedBy is null.");
                throw new ArgumentNullException(nameof(rejectedBy));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new ReopenRejectedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await notificationBuilder.Build(cost, costUsers, rejectedBy.FullName, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostReopenRequestedNotification(
            CostNotificationUsers costUsers, Cost cost, CostStageRevision costStageRevision, DateTime timestamp, CostUser requestedBy
            )
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (costUsers.ClientAdmins == null || !costUsers.ClientAdmins.Any())
            {
                Logger.Error("Failed to create an email notification because costUsers is null or empty.");
                throw new ArgumentNullException(nameof(costUsers.ClientAdmins));
            }

            if (requestedBy == null)
            {
                Logger.Error("Failed to create an email notification because rejectedBy is null.");
                throw new ArgumentNullException(nameof(requestedBy));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            var notificationBuilder = new ReopenRequestedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await notificationBuilder.Build(cost, costUsers, requestedBy.FullName, costStageRevision, timestamp);
        }

        public async Task<IEnumerable<EmailNotificationMessage<CostNotificationObject>>> BuildCostOwnerChangedNotification(CostNotificationUsers costUsers,
            Cost cost,
            CostStageRevision costStageRevision,
            DateTime timestamp,
            CostUser changeApprover,
            CostUser previousOwner)
        {
            if (cost == null)
            {
                Logger.Error("Failed to create an email notification because cost is null.");
                throw new ArgumentNullException(nameof(cost));
            }

            if (costStageRevision == null)
            {
                Logger.Error("Failed to create an email notification because costStageRevision is null.");
                throw new ArgumentNullException(nameof(costStageRevision));
            }

            if (timestamp == DateTime.MinValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MinValue");
                throw new ArgumentException("Param cannot be DateTime.MinValue", nameof(timestamp));
            }

            if (timestamp == DateTime.MaxValue)
            {
                Logger.Error("Failed to create an email notification because system timestamp provided was DateTime.MaxValue");
                throw new ArgumentException("Param cannot be DateTime.MaxValue", nameof(timestamp));
            }

            if (changeApprover == null)
            {
                Logger.Error("Failed to create an email notification because changeApprover is null.");
                throw new ArgumentNullException(nameof(changeApprover));
            }

            if (previousOwner == null)
            {
                Logger.Error("Failed to create an email notification because previousOwner is null.");
                throw new ArgumentNullException(nameof(previousOwner));
            }

            var notificationBuilder = new OwnerChangedNotificationBuilder(_mapper,
                _uriHelper, _costStageRevisionService, _metadataProviderService, _appSettings, _efContext, _regionsService);
            return await notificationBuilder.Build(cost, costUsers, costStageRevision, timestamp, changeApprover, previousOwner);
        }
    }
}

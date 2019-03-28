namespace costs.net.plugins.PG.Builders.Notifications
{
    using AutoMapper;
    using core.Helpers;
    using core.Models.Notifications;
    using core.Services.Costs;
    using dataAccess.Entity;
    using Form;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using MoreLinq;
    using Cost = dataAccess.Entity.Cost;

    abstract class NotificationBuilderBase
    {
        private readonly IApplicationUriHelper _uriHelper;
        protected readonly IMapper Mapper;
        protected readonly ICostStageRevisionService CostStageRevisionService;
        protected readonly IMetadataProviderService MetadataProviderService;
        protected readonly AppSettings AppSettings;
        protected readonly EFContext EFContext;
        protected readonly IRegionsService RegionsService;

        protected NotificationBuilderBase(IMapper mapper,
            IApplicationUriHelper uriHelper, ICostStageRevisionService costStageRevisionService,
            IMetadataProviderService metadataProviderService,
            AppSettings appSettings,
            EFContext efContext,
            IRegionsService regionsService)
        {
            Mapper = mapper;
            _uriHelper = uriHelper;
            CostStageRevisionService = costStageRevisionService;
            MetadataProviderService = metadataProviderService;
            AppSettings = appSettings;
            EFContext = efContext;
            RegionsService = regionsService;
        }

        protected void MapEmailNotificationObject(CostNotificationObject obj, Cost cost, CostUser costOwner, CostUser costApprover = null)
        {
            var users = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Approver = costApprover
            };
            MapEmailNotificationObject(obj, cost, users);
        }

        protected void MapEmailNotificationObject(CostNotificationObject obj, Cost cost, CostNotificationUsers users)
        {
            dataAccess.Entity.Project project = cost.Project;

            obj.Agency = Mapper.Map<core.Models.Notifications.Agency>(users.CostOwner.Agency);
            Mapper.Map(users.CostOwner.Agency.Country, obj.Agency);

            obj.Brand = Mapper.Map<core.Models.Notifications.Brand>(project.Brand);

            obj.Cost = Mapper.Map<core.Models.Notifications.Cost>(cost);
            Mapper.Map(cost.LatestCostStageRevision.CostStage, obj.Cost);
            Mapper.Map(users.CostOwner, obj.Cost);

            obj.Project = Mapper.Map<core.Models.Notifications.Project>(project);

            if (users.Approver != null)
            {
                obj.Approver = Mapper.Map<Approver>(users.Approver);
            }
        }

        protected void AddSharedTo(EmailNotificationMessage<CostNotificationObject> message)
        {
            var users = EFContext.CostUser.Include(cu => cu.Agency).Where(cu => message.Recipients.Contains(cu.GdamUserId)).ToList();
            message.Recipients.ForEach(gdamUserId =>
            {
                var selectedUser = users.FirstOrDefault(a => a.GdamUserId == gdamUserId);
                message.Action.Share.To.Add(new To
                {
                    Id = gdamUserId,
                    FullName = selectedUser?.FullName,
                    Agency = new EmailAgency
                    {
                        // This overrides Agency URL but only for adcosts (its a known behaviour) and only when it exists!
                        Url = selectedUser?.EmailUrl ?? selectedUser?.Agency?.NotificationUrl
                    }
                });
            });

        }

        protected void PopulateOtherFields(EmailNotificationMessage<CostNotificationObject> message, string parent, DateTime timestamp, Guid costId, Guid costStageRevisionId, CostUser approvalCostUser = null)
        {
            message.Timestamp = timestamp;

            CostNotificationObject obj = message.Object;
            core.Models.Notifications.Cost cost = obj.Cost;

            PgStageDetailsForm stageForm = CostStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId).Result;
            cost.Title = stageForm.Title;
            switch (cost.CostType)
            {
                case core.Models.CostTemplate.CostType.Production:
                    cost.ProductionType = stageForm.ProductionType?.Key;
                    cost.ContentType = stageForm.ContentType.Key;
                    break;
                case core.Models.CostTemplate.CostType.Buyout:
                    cost.ProductionType = stageForm.UsageBuyoutType.Key;
                    cost.ContentType = stageForm.UsageType?.Key;
                    break;
            }

            cost.Url = _uriHelper.GetLink(ApplicationUriName.CostRevisionReview, costStageRevisionId.ToString(), costId.ToString());

            AssignApproverType(message, approvalCostUser);

            if (!string.IsNullOrEmpty(parent))
            {
                obj.Parents.Add(parent);
            }
            if (!string.IsNullOrEmpty(AppSettings.EnvironmentEmailSubjectPrefix))
            {
                obj.EnvironmentEmailSubjectPrefix = AppSettings.EnvironmentEmailSubjectPrefix;
            }
        }

        protected async Task PopulateMetadata(CostNotificationObject obj, Guid costId)
        {
            var metadataItems = await MetadataProviderService.Provide(costId);

            metadataItems?.ForEach(item => obj.Metadata.Add(item));
        }

        private void AssignApproverType(EmailNotificationMessage<CostNotificationObject> message, CostUser approvalCostUser = null)
        {
            CostNotificationObject obj = message.Object;

            if (message.Type == core.Constants.EmailNotificationActionType.TechnicalApproverAssigned)
            {
                //IPM should be default
                obj.Approver.Type = (approvalCostUser?.UserBusinessRoles == null
                    || approvalCostUser.UserBusinessRoles.Any(br => br.BusinessRole != null && br.BusinessRole.Key == Constants.BusinessRole.Ipm))
                        ? core.Constants.EmailApprovalType.IPM
                        : core.Constants.EmailApprovalType.CC;
            }
            if (message.Type == core.Constants.EmailNotificationActionType.BrandApproverAssigned)
            {
                obj.Approver.Type = core.Constants.EmailApprovalType.Brand;
            }
        }

        protected string GetApprovalType(Approval approval)
        {
            if (approval.Type == ApprovalType.IPM)
            {
                return core.Constants.EmailApprovalType.IPM;
            }

            return core.Constants.EmailApprovalType.Brand;
        }

        protected bool IsNorthAmericanBudgetRegion(Guid costStageRevisionId)
        {
            var stageForm = CostStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId).Result;

            return stageForm.BudgetRegion?.Key == Constants.BudgetRegion.NorthAmerica;
        }

        protected string GetBudgetRegion(CostStageRevision costStageRevision)
        {
            var pgStageDetailsForm = CostStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevision.Id).Result;
            return pgStageDetailsForm.BudgetRegion?.Key;
        }

        protected bool ShouldNotifyInsuranceUsers(CostNotificationUsers costUsers)
        {
            var costOwner = costUsers.CostOwner;
            var agency = costOwner.Agency;
            return costUsers.InsuranceUsers?.Count > 0 && (agency.IsEuropeanAgency(RegionsService) || agency.IsNorthAmericanAgency(RegionsService));
        }

        protected async Task<EmailNotificationMessage<CostNotificationObject>> AddFinanceManagerNotification(
            string actionType,
            Cost cost,
            CostNotificationUsers costUsers,
            CostStageRevision costStageRevision,
            DateTime timestamp,
            List<EmailNotificationMessage<CostNotificationObject>> notifications
            )
        {
            //Only send to North American P&G Finance Users when Budget Region is North America and Cyclone costs
            if (costUsers.FinanceManagementUsers == null
                || !costUsers.FinanceManagementUsers.Any()
                || Constants.BudgetRegion.NorthAmerica != GetBudgetRegion(costStageRevision)
                || !costUsers.CostOwner.Agency.IsCyclone())
            {
                return null;
            }

            //Send to FinanceManagement as well
            var financeManagementNotification =
                new EmailNotificationMessage<CostNotificationObject>(actionType, costUsers.FinanceManagementUsers);
            var notificationObject = new FinanceManagerCostNotificationObject();
            financeManagementNotification.Object = notificationObject;
            var parent = core.Constants.EmailNotificationParents.FinanceManagement;
            AddSharedTo(financeManagementNotification);
            MapEmailNotificationObject(financeManagementNotification.Object, cost, costUsers);
            PopulateOtherFields(financeManagementNotification, parent, timestamp, cost.Id, costStageRevision.Id);
            await PopulateMetadata(financeManagementNotification.Object, cost.Id);
            notifications.Add(financeManagementNotification);

            if (cost.Status == CostStageRevisionStatus.Approved && cost.LatestCostStageRevision.CostStage.Key == Models.Stage.CostStages.OriginalEstimate.ToString())
            {
                notificationObject.CanAssignIONumber = true;
            }

            return financeManagementNotification;
        }
    }
}

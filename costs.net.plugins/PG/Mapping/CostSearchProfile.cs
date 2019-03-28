namespace costs.net.plugins.PG.Mapping
{
    using AutoMapper;
    using core.Builders.Response;
    using core.Events.Cost;
    using costs.net.core.Models.Approvals;
    using costs.net.core.Models.Costs;
    using costs.net.dataAccess.Views;
    using costs.net.plugins.PG.Models;
    using dataAccess.Entity;
    using Form;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using ApprovalModel = core.Models.Costs.ApprovalModel;

    public class CostSearchProfile : Profile
    {
        public CostSearchProfile()
        {
            //TODO add the rest of the fields: city, costOwner, country, CostNumber, Initiative, IoNumber, CostNumber, UserGroups

            CreateMap<ProductionDetailsUpdated, CostSearchItem>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.AggregateId))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<StageDetailsUpdated, CostSearchItem>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.AggregateId))
                .ForAllOtherMembers(m => m.Ignore());

            // TODO add mapping if any field from ProductionDetails is needed in search 
            CreateMap<PgProductionDetailsForm, CostSearchItem>()
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<Cost, CostSearchItem>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CreatedBy, opt => opt.MapFrom(s => s.CreatedById.ToString()))
                .ForMember(d => d.OwnerId, opt => opt.MapFrom(s => s.OwnerId.ToString()))
                .ForMember(d => d.UserModifiedDate, opt => opt.MapFrom(s => s.UserModified))
                .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => s.Created))
                .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
                .ForMember(d => d.AgencyName, opt =>
                {
                    opt.PreCondition(s => !string.IsNullOrEmpty(s.Owner?.Agency.Name));
                    opt.MapFrom(s => s.Owner.Agency.Name);
                })
                .ForMember(d => d.CostNumber, opt => opt.MapFrom(s => s.CostNumber))
                .ForMember(d => d.CostOwner, opt => opt.MapFrom(s => s.Owner.FullName))
                .ForMember(d => d.ProjectId, opt => opt.MapFrom(s => s.ProjectId))
                .ForMember(d => d.CostType, opt => opt.MapFrom(s => s.CostType.ToString()))
                .ForMember(d => d.AgencyId, opt => opt.MapFrom(s => s.ParentId))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<CostCreated, CostSearchItem>()
                .ForMember(d => d.CreatedBy, opt => opt.MapFrom(s => s.CreatedByEmail))
                .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.AggregateId))
                .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<PgStageDetailsForm, CostSearchItem>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.AgencyProducer, opt => opt.MapFrom(s => s.AgencyProducer))
                .ForMember(d => d.AgencyTrackingNumber, opt => opt.MapFrom(s => s.AgencyTrackingNumber))
                .ForMember(d => d.BudgetRegion, opt =>
                {
                    opt.PreCondition(s => s.BudgetRegion != null);
                    opt.MapFrom(s => s.BudgetRegion.Key);
                })
                .ForMember(d => d.BudgetRegionName, opt =>
                {
                    opt.PreCondition(s => s.BudgetRegion != null);
                    opt.MapFrom(s => s.BudgetRegion.Name);
                })
                .ForMember(d => d.Budget, opt => opt.MapFrom(s => s.InitialBudget))
                .ForMember(d => d.ContentType, opt =>
                {
                    opt.PreCondition(s => s.ContentType != null);
                    opt.MapFrom(s => s.ContentType.Key);
                })
                .ForMember(d => d.ContentTypeValue, opt =>
                {
                    opt.PreCondition(s => s.ContentType != null);
                    opt.MapFrom(s => s.ContentType.Value);
                })
                .ForMember(d => d.ProductionType, opt =>
                {
                    opt.PreCondition(c => c.ProductionType != null);
                    opt.MapFrom(s => s.ProductionType.Key);
                })
                .ForMember(d => d.ProductionTypeValue, opt =>
                {
                    opt.PreCondition(c => c.ProductionType != null);
                    opt.MapFrom(s => s.ProductionType.Value);
                })
                .ForMember(d => d.Campaign, opt => opt.MapFrom(s => s.Campaign))
                .ForMember(d => d.UsageType, opt =>
                {
                    opt.PreCondition(c => c.UsageType != null);
                    opt.MapFrom(s => s.UsageType.Key);
                })
                .ForMember(d => d.UsageBuyoutType, opt =>
                {
                    opt.PreCondition(s => s.UsageBuyoutType != null);
                    opt.MapFrom(s => s.UsageBuyoutType.Key);
                })
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<Currency, CostSearchItem>()
                .ForMember(d => d.CurrencySymbol, opt => opt.MapFrom(s => s.Symbol))
                .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Code))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<Project, ProjectViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.GdamProjectId, opt => opt.MapFrom(s => s.GdamProjectId))
                .ForMember(d => d.AdCostNumber, opt => opt.MapFrom(s => s.AdCostNumber));

            CreateMap<CostLineItem, CostLineItemView>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.CostStageRevisionId, opt => opt.MapFrom(s => s.CostStageRevisionId))
                .ForMember(d => d.TemplateSectionId, opt => opt.MapFrom(s => s.TemplateSectionId))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.ValueInDefaultCurrency, opt => opt.MapFrom(s => s.ValueInDefaultCurrency))
                .ForMember(d => d.ValueInLocalCurrency, opt => opt.MapFrom(s => s.ValueInLocalCurrency))
                .ForMember(d => d.LocalCurrencyId, opt => opt.MapFrom(s => s.LocalCurrencyId))
                .ForMember(d => d.CreatedById, opt => opt.MapFrom(s => s.CreatedById))
                .ForMember(d => d.Created, opt => opt.MapFrom(s => s.Created))
                .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.Modified))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<CostFormDetails, CostFormDetailModel>()
                .ForMember(d => d.FormDataId, opt => opt.MapFrom(s => s.FormDataId))
                .ForMember(d => d.FormDefinitionId, opt => opt.MapFrom(s => s.FormDefinitionId))
                .ForMember(d => d.CustomFormData, opt => opt.MapFrom(s => s.CustomFormData == null
                            ? null
                            : new CustomFormDataModel
                            {
                                Id = s.CustomFormData.Id,
                                Data = JToken.Parse(s.CustomFormData.Data)
                            }));

            CreateMap<CustomObjectData, CustomObjectDataModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.ObjectId, opt => opt.MapFrom(s => s.ObjectId))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.Data, opt => opt.MapFrom(s => s.Data));

            CreateMap<CostStageRevision, CostStageRevisionViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.CostStage.Name))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.IsPaymentCurrencyLocked, opt => opt.MapFrom(s => s.IsPaymentCurrencyLocked))
                .ForMember(d => d.IsLineItemSectionCurrencyLocked, opt => opt.MapFrom(s => s.IsLineItemSectionCurrencyLocked))
                .ForMember(d => d.Submitted, opt => opt.MapFrom(s => s.Submitted))
                .ForMember(d => d.CostId, opt => opt.MapFrom(s => s.CostStage.CostId))
                .ForMember(d => d.StageId, opt => opt.MapFrom(s => s.CostStageId))
                .ForMember(d => d.CostLineItems, opt => opt.MapFrom(s => s.CostLineItems))
                .ForMember(d => d.SupportingDocuments, opt => opt.MapFrom(s => s.SupportingDocuments))
                .ForMember(d => d.CustomDataModel, opt => opt.MapFrom(s => s.CustomObjectData))
                .ForMember(d => d.CostFormDetails, opt => opt.MapFrom(s => s.CostFormDetails))
                .ForMember(d => d.StageDetails, opt => opt.MapFrom(s => s.StageDetails == null
                            ? null
                            : new CustomFormDataModel
                            {
                                Id = s.StageDetails.Id,
                                Data = JToken.Parse(s.StageDetails.Data)
                            }))
                .ForMember(d => d.ProductionDetails, opt => opt.MapFrom(s => s.ProductDetails == null
                            ? null
                            : new CustomFormDataModel
                            {
                                Id = s.ProductDetails.Id,
                                Data = JToken.Parse(s.ProductDetails.Data)
                            }))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<CostStage, CostStageLatestModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Key, opt => opt.MapFrom(s => s.Key))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.StageOrder, opt => opt.MapFrom(s => s.StageOrder));

            CreateMap<ApprovalMember, ApprovalModel.Member>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.CostUser.FullName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.CostUser.Email))
                .ForMember(d => d.ApprovalLimit, opt => opt.MapFrom(s => s.CostUser.ApprovalLimit.ToString()))
                .ForMember(d => d.ApprovalBandId, opt => opt.MapFrom(s => s.CostUser.ApprovalBandId.ToString()))
                .ForMember(d => d.BusinessRoles, opt => opt.MapFrom(s => s.CostUser.UserBusinessRoles.Select(ubr => ubr.BusinessRole.Value).Distinct()))
                .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.RejectionDetails.Comments))
                .ForMember(d => d.RejectionTimestamp, opt => opt.MapFrom(s => s.RejectionDetails.Created))
                .ForMember(d => d.ApprovalTimestamp, opt => opt.MapFrom(s => s.ApprovalDetails.Created))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<Requisitioner, RequisitionerModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.CostUser.FullName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.CostUser.Email))
                .ForMember(d => d.BusinessRoles, opt => opt.MapFrom(s => s.CostUser.UserBusinessRoles.Select(ubr => ubr.BusinessRole.Value).Distinct()))
                .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.RejectionDetails.Comments))
                .ForMember(d => d.RejectionTimestamp, opt => opt.MapFrom(s => s.RejectionDetails.Created))
                .ForMember(d => d.ApprovalTimestamp, opt => opt.MapFrom(s => s.ApprovalDetails.Created))
                .ForAllOtherMembers(m => m.Ignore());

            CreateMap<Approval, ApprovalModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(d => d.Created, opt => opt.MapFrom(s => s.Created))
                .ForMember(d => d.CreatedById, opt => opt.MapFrom(s => s.CreatedById))
                .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.Modified))
                .ForMember(d => d.ValidBusinessRoles, opt => opt.MapFrom(s => s.ValidBusinessRoles))
                .ForMember(d => d.IsExternal, opt => opt.MapFrom(s => s.ApprovalMembers.Any(am => am.IsExternal)))
                .ForMember(d => d.Requisitioners, opt => opt.MapFrom(s => s.Requisitioners))
                .ForMember(d => d.ApprovalMembers, opt => opt.MapFrom(s => s.ApprovalMembers.Where(am => !am.IsExternal)))
                .ForAllOtherMembers(m => m.Ignore());
        }
    }
}

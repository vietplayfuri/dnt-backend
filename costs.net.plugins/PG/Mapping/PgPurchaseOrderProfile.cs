namespace costs.net.plugins.PG.Mapping
{
    using AutoMapper;
    using core.Models.Notifications;
    using Models;
    using Models.PurchaseOrder;

    public class PgPurchaseOrderProfile : Profile
    {
        public PgPurchaseOrderProfile()
        {
            CreateMap<PgPurchaseOrderResponse, PgPaymentDetails>()
                .ForMember(d => d.PoNumber, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.PoNumber));
                    opt.MapFrom(src => src.PoNumber);
                })
                .ForMember(d => d.GrNumber, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.GrNumber));
                    opt.MapFrom(src => src.GrNumber);
                })             
				  .ForMember(d => d.Requisition, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.Requisition));
                    opt.MapFrom(src => src.Requisition);
                })
                .ForMember(d => d.IoNumberOwner, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.IoNumberOwner));
                    opt.MapFrom(src => src.IoNumberOwner);
                })
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<PgPurchaseOrderResponse, PgPurchaseOrderResponse>()
                .ForMember(d => d.Requisition, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.Requisition));
                    opt.MapFrom(src => src.Requisition);
                })
                .ForMember(d => d.PoNumber, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.PoNumber));
                    opt.MapFrom(src => src.PoNumber);
                })
                .ForMember(d => d.GrNumber, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.GrNumber));
                    opt.MapFrom(src => src.GrNumber);
                })
                .ForMember(d => d.GlAccount, (opt) =>
                {
                    opt.Condition(res => !string.IsNullOrEmpty(res.GlAccount));
                    opt.MapFrom(src => src.GlAccount);
                })
                .ForMember(d => d.PoDate, opt =>
                {
                    opt.Condition(src => src.PoDate.HasValue);
                    opt.MapFrom(src => src.PoDate);
                })
                .ForMember(d => d.GrDate, opt =>
                {
                    opt.Condition(src => src.GrDate.HasValue);
                    opt.MapFrom(src => src.PoDate);
                })
                .ForMember(d=> d.AccountCode, opt=>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.AccountCode));
                    opt.MapFrom(src => src.AccountCode);
                })
                .ForMember(d => d.IoNumberOwner, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.IoNumberOwner));
                    opt.MapFrom(src => src.IoNumberOwner);
                })
                .ForMember(d => d.Comments, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.Comments));
                    opt.MapFrom(src => src.Comments);
                })
                .ForMember(d => d.Type, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.Type));
                    opt.MapFrom(src => src.Type);
                })
                .ForMember(d => d.ItemIdCode, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.ItemIdCode));
                    opt.MapFrom(src => src.ItemIdCode);
                })
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<PgPaymentDetails, PurchaseOrder>()
                .ForMember(d => d.PoNumber, opt =>
                {
                    opt.Condition(src => !string.IsNullOrEmpty(src.PoNumber));
                    opt.MapFrom(src => src.PoNumber);
                })
                .ForMember(d => d.TotalAmount, opt => opt.Ignore());
        }
    }
}



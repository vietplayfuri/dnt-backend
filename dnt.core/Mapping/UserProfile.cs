namespace dnt.core.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using dnt.dataAccess.Entity;
    using Models.User;

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Fullname))
                .ForMember(d => d.Username, opt => opt.MapFrom(s => s.Username));

            CreateMap<UserInsertModel, User>()
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.Fullname, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.Username, opt => opt.MapFrom(s => s.Username))
                .ForMember(d => d.Password, opt => opt.MapFrom(s => s.Password))
                .ForAllOtherMembers(m => m.Ignore());
        }

        //private class UserAgencyResolver : IValueResolver<CostUser, ICostUserModel, AgencyModel>
        //{
        //    public AgencyModel Resolve(CostUser source, ICostUserModel destination, AgencyModel destMember, ResolutionContext context)
        //    {
        //        return source?.Agency == null
        //            ? null
        //            : new AgencyModel
        //            {
        //                Id = source.AgencyId,
        //                Name = source.Agency.Name,
        //                CountryId = source.Agency.CountryId,
        //                CountryName = source.Agency.Country?.Name,
        //                CountryIso = source.Agency.Country?.Iso,
        //                Labels = source.Agency.Labels,
        //                IsCostModuleOwner = source.Agency.Labels.Any(a => a.StartsWith(core.Constants.BusinessUnit.CostModulePrimaryLabelPrefix))
        //            };
        //    }
        //}


    }
}

namespace costs.net.plugins.PG.Services.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Role;
    using dataAccess;
    using dataAccess.Entity;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public class PgRoleService : IPluginRoleService
    {
        private readonly AppSettings _appSettings;
        private readonly string[] _costOwnerBusinessRoles =
        {
            Constants.BusinessRole.Ipm,
            Constants.BusinessRole.BrandManager,
            Constants.BusinessRole.FinanceManager,
            Constants.BusinessRole.PurchasingSupport,
            Constants.BusinessRole.InsuranceUser,
            Constants.BusinessRole.RegionSupport,
            Constants.BusinessRole.GovernanceManager
        };

        private readonly string[] _agencyUserBusinessRoles =
        {
            Constants.BusinessRole.RegionalAgencyUser,
            Constants.BusinessRole.AgencyOwner,
            Constants.BusinessRole.AgencyFinanceManager,
            Constants.BusinessRole.AgencyAdmin,
            Constants.BusinessRole.CentralAdaptationSupplier
        };

        private readonly EFContext _efContext;

        public PgRoleService(EFContext efContext, IOptions<AppSettings> options)
        {
            _efContext = efContext;
            _appSettings = options.Value;
        }

        public async Task<List<BusinessRole>> GetBusinessRoles(Guid currentUserId, Guid userId)
        {
            if (currentUserId.ToString() == _appSettings.CostsAdminUserId)
            {
                return await _efContext.BusinessRole.ToListAsync();
            }

            List<BusinessRole> businessRoles;
            var user = await _efContext.CostUser.Include(cu => cu.Agency).FirstOrDefaultAsync(cu => cu.Id == userId);
            if (user.Agency.IsCostModuleOwner())
            {
                businessRoles = await _efContext.BusinessRole.Where(br => _costOwnerBusinessRoles.Contains(br.Key)).ToListAsync();
            }
            else
            {
                businessRoles = await _efContext.BusinessRole.Where(br => _agencyUserBusinessRoles.Contains(br.Key)).ToListAsync();
            }

            // AdstreamAdmin always linked to root abstract_type therefore do not need to check objectId
            var currentUser = await _efContext.CostUser
                .Include(u => u.UserBusinessRoles)
                    .ThenInclude(ubr => ubr.BusinessRole)
                .Where(cu => cu.Id == currentUserId && cu.UserBusinessRoles.Any(ubr => ubr.BusinessRole.Key == Constants.BusinessRole.AdstreamAdmin))
                .FirstOrDefaultAsync();

            if (currentUser != null)
            {
                var businessRole = currentUser.UserBusinessRoles.Select(ubr => ubr.BusinessRole).First(br => br.Key == Constants.BusinessRole.AdstreamAdmin);
                businessRoles.Add(businessRole);
            }

            return businessRoles;
        }
    }
}
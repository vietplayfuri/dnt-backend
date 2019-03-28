namespace costs.net.plugins.PG.Builders.Requisitioner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Requisitioner;
    using core.Models.Approvals;
    using dataAccess;
    using Microsoft.EntityFrameworkCore;

    public class PgRequisitionerBuilder : IRequisitionerBuilder
    {
        private readonly EFContext _efContext;
        private readonly string[] _requisitionerRoles = { Constants.BusinessRole.BrandManager };

        public PgRequisitionerBuilder(EFContext efContext)
        {
            _efContext = efContext;
        }

        public async Task<List<RequisitionerModel>> GetRequisitioners()
        {
            var requisitioners = await _efContext.CostUser
                .Include(a => a.UserBusinessRoles)
                .ThenInclude(a => a.BusinessRole)
                .Where(u => u.UserBusinessRoles.Any(a => _requisitionerRoles.Contains(a.BusinessRole.Key)))
                .OrderBy(r=>r.FirstName)
                .ToListAsync();

            return requisitioners
                .Select(r => new RequisitionerModel
                {
                    Id = r.Id,
                    Email = r.Email,
                    FullName = r.FullName,
                    BusinessRoles = r.UserBusinessRoles.Select(a => a.BusinessRole.Value).Distinct()
                })
                .ToList();
        }
    }
}
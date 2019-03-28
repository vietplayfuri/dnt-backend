namespace costs.net.plugins.PG.Builders.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Response;
    using core.Builders.Search;
    using core.Models.CostUser;
    using dataAccess;
    using Microsoft.EntityFrameworkCore;
    using Nest;

    public class PgUserQueryBuilder : IUserQueryBuilder
    {
        private readonly EFContext _efContext;

        public PgUserQueryBuilder(EFContext efContext)
        {
            _efContext = efContext;
        }

        public async Task<List<QueryContainer>> GetQueryContainers(QueryContainerDescriptor<CostUserSearchItem> queryDescriptor, CostUserQuery query, Guid userId)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var costUser = await _efContext.CostUser
                .Include(u => u.UserBusinessRoles)
                .ThenInclude(ubr => ubr.BusinessRole)
                .FirstAsync(u => u.Id == userId);

            var businessRoles = costUser.UserBusinessRoles.Select(ubr => ubr.BusinessRole).Select(br => br.Key).ToHashSet();

            var filterQueries = new List<QueryContainer>();
            if (businessRoles.Contains(Constants.BusinessRole.AgencyAdmin))
            {
                filterQueries.Add(queryDescriptor.Term(t => t.Field(c => c.Agency.Id).Value(costUser.AgencyId.ToString())));
            }
            else if (query.AgencyId != Guid.Empty)
            {
                filterQueries.Add(queryDescriptor.Term(t => t.Field(c => c.Agency.Id).Value(query.AgencyId)));
            }

            if (businessRoles.Contains(Constants.BusinessRole.GovernanceManager))
            {
                filterQueries.Add(queryDescriptor.Term(t => t.Field(c => c.IsCostModuleOwner).Value(true)));
            }

            return filterQueries;
        }
    }
}
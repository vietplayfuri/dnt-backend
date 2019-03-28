namespace costs.net.plugins.PG.Services.Agency
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Extensions;
    using core.Services;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;

    public class PgAgencyService : IPgAgencyService
    {
        private readonly EFContext _efContext;
        private readonly IPermissionService _permissionService;

        public PgAgencyService(EFContext efContext, IPermissionService permissionService)
        {
            _efContext = efContext;
            _permissionService = permissionService;
        }

        /// <summary>
        ///     Returns the list of updated/created pseudo agencies
        /// </summary>
        /// <param name="agencies"></param>
        /// <returns></returns>
        public async Task<List<AbstractType>> GetOrCreatePseudoAgencies(Agency[] agencies)
        {
            if (agencies == null)
            {
                throw new ArgumentNullException(nameof(agencies));
            }

            if (agencies.Length == 0)
            {
                return new List<AbstractType>();
            }

            var pgBusinessUnits = await _efContext.AbstractType
                .Include(a => a.Agency)
                .Where(at => at.Agency != null && at.Agency.Labels.Any(a => a.Equals(Constants.Agency.PgOwnerLabel)))
                .ToListAsync();

            var preloadedAbstractTypes = await _efContext.AbstractType
                .Include(a => a.Agency)
                .Where(at => pgBusinessUnits.Any(pbu => pbu.Id == at.ParentId))
                .ToDictionaryAsync(at => new Tuple<Guid, Guid>(at.ParentId, at.ObjectId), at => at);

            var pseudoAgencies = new List<AbstractType>();
            foreach (var bu in pgBusinessUnits)
            {
                foreach (var agency in agencies)
                {
                    var agencyAbstractType = await CreateOrUpdatePseudoAgency(bu, agency, preloadedAbstractTypes);
                    pseudoAgencies.Add(agencyAbstractType);
                }
            }

            return pseudoAgencies;
        }

        /// <summary>
        ///     Returns affected AbstractType
        /// </summary>
        /// <param name="bu"></param>
        /// <param name="agency"></param>
        /// <param name="preloadedAbstractTypes"></param>
        /// <returns></returns>
        public async Task<AbstractType> CreateOrUpdatePseudoAgency(
            AbstractType bu, Agency agency, Dictionary<Tuple<Guid, Guid>, AbstractType> preloadedAbstractTypes)
        {
            var key = new Tuple<Guid, Guid>(bu.Id, agency.Id);
            AbstractType agencyAbstractType;

            if (!preloadedAbstractTypes.ContainsKey(key))
            {
                agencyAbstractType = await AddAgencyAbstractType(agency, bu);
                preloadedAbstractTypes.Add(key, agencyAbstractType);
            }
            else
            {
                agencyAbstractType = preloadedAbstractTypes[key];
            }

            return agencyAbstractType;
        }

        public async Task<AbstractType> AddAgencyAbstractType(Agency agency, AbstractType abstractType)
        {
            var newAbstractType = new AbstractType
            {
                Id = Guid.NewGuid(),
                Agency = agency,
                ParentId = abstractType.Id,
                Type = AbstractObjectType.Agency.ToString()
            };
            _efContext.AbstractType.Add(newAbstractType);
            newAbstractType.UserGroups = await _permissionService.CreateDomainNode(
                typeof(AbstractType).Name.ToSnakeCase(), 
                newAbstractType.ParentId, 
                newAbstractType.Id
                );
            return newAbstractType;
        }
    }
}
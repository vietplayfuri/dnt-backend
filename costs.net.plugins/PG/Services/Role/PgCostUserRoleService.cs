namespace costs.net.plugins.PG.Services.Role
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;

    public class PgCostUserRoleService : IPgCostUserRoleService
    {
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly EFContext _efContext;

        public PgCostUserRoleService(EFContext efContext, ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<bool> DoesUserHaveRoleForCost(Guid costUserId, Guid costId, string businessRole)
        {
            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr => csr.StageDetails)
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (cost == null)
            {
                return false;
            }

            var costUser = await _efContext.CostUser
                .Include(cu => cu.UserBusinessRoles)
                    .ThenInclude(ubr => ubr.BusinessRole)
                .FirstOrDefaultAsync(cu => cu.Id == costUserId);

            if (costUser == null)
            {
                return false;
            }

            return DoesUserHaveRoleForCost(costUser, cost.LatestCostStageRevision, businessRole);
        }

        public bool DoesUserHaveRoleForCost(CostUser costUser, CostStageRevision costStageRevision, string businessRole)
        {
            if (costUser == null)
            {
                throw new ArgumentNullException(nameof(costUser));
            }

            if (costUser.UserBusinessRoles == null)
            {
                throw new ArgumentException($"{nameof(costUser.UserBusinessRoles)} are missing");
            }

            if (costStageRevision == null)
            {
                throw new ArgumentException($"{nameof(costStageRevision)} is missing");
            }

            if (costStageRevision.StageDetails == null)
            {
                throw new ArgumentException($"{nameof(costStageRevision.StageDetails)} is missing");
            }

            var stageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevision);
            var smoName = stageDetails.SmoName;

            var hasRole = costUser.UserBusinessRoles.Any(ubr =>
                ubr.BusinessRole != null
                && ubr.BusinessRole.Key == businessRole
                && (ubr.ObjectId != null || ubr.Labels.Contains(smoName))
            );

            return hasRole;
        }
    }
}
namespace costs.net.plugins.PG.Services.Role
{
    using System;
    using System.Threading.Tasks;
    using dataAccess.Entity;

    public interface IPgCostUserRoleService
    {
        /// <summary>
        ///     Checks if the user has specified business role against cost. Loads costUser and cost from database.
        /// </summary>
        /// <param name="costUserId"></param>
        /// <param name="costId"></param>
        /// <param name="businessRole"></param>
        /// <returns></returns>
        Task<bool> DoesUserHaveRoleForCost(Guid costUserId, Guid costId, string businessRole);

        /// <summary>
        ///     Checks if the user has specified business role against cost.
        /// </summary>
        /// <param name="costUser"></param>
        /// <param name="costStageRevision"></param>
        /// <param name="businessRole"></param>
        /// <returns></returns>
        bool DoesUserHaveRoleForCost(CostUser costUser, CostStageRevision costStageRevision, string businessRole);
    }
}
namespace costs.net.plugins.PG.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.CustomData;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Serilog;

    public class CostUserService : ICostUserService
    {
        private static readonly ILogger Logger = Log.ForContext<CostUserService>();

        private readonly EFContext _efContext;
        private readonly ICustomObjectDataService _customObjectDataService;
        private readonly IRegionsService _regionsService;

        public CostUserService(EFContext efContext, IRegionsService regionsService,
            ICustomObjectDataService customObjectDataService)
        {
            _efContext = efContext;
            _regionsService = regionsService;
            _customObjectDataService = customObjectDataService;
        }

        public async Task<List<string>> GetInsuranceUsers(Agency agency)
        {
            //if agency is north american, get the north america area region insurance user
            if (agency.IsNorthAmericanAgency(_regionsService))
            {
                var region = await _efContext.Region.FirstOrDefaultAsync(a =>
                   a.Key == Constants.Region.NorthAmericanArea);

                if (region == null)
                {
                    Logger.Warning(
                        $"Parent not found for Type: {core.Constants.AccessObjectType.Region} and Key:{Constants.Region.NorthAmericanArea}. As a result, Insurance User cannot be found.");
                    return null;
                }

                return await ReturnAllUserGdamIds(Constants.BusinessRole.InsuranceUser, core.Constants.AccessObjectType.Region);

            }
            //if agency is european, get the western europe smo insurance user
            if (agency.IsEuropeanAgency(_regionsService))
            {
                var abstractType = await _efContext.Smo.FirstOrDefaultAsync(
                    a => a.Key == Constants.Smo.WesternEurope);

                if (abstractType == null)
                {
                    Logger.Warning(
                        $"Parent not found for Type: {core.Constants.AccessObjectType.Region} and Key:{Constants.Region.NorthAmericanArea}. As a result, Insurance User cannot be found.");
                    return null;
                }

                return await ReturnAllUserGdamIds(Constants.BusinessRole.InsuranceUser, core.Constants.AccessObjectType.Smo);
            }

            //otherwise return null
            return null;
        }

        public async Task<IEnumerable<string>> GetFinanceManagementUsers(IEnumerable<string> userGroups, string budgetRegion)
        {
            var financeManagerBusinessRole = _efContext.BusinessRole.Include(br => br.Role).FirstOrDefault(a => a.Key == Constants.BusinessRole.FinanceManager);
            var guids = userGroups.Select(Guid.Parse).ToList();
            return await _efContext.CostUser
                .Where(u =>
                    u.NotificationBudgetRegion != null &&
                    u.NotificationBudgetRegion.Key == budgetRegion &&
                    u.UserBusinessRoles.Any(ubr => ubr.BusinessRoleId == financeManagerBusinessRole.Id) &&
                    u.UserUserGroups.Any(uug => guids.Contains(uug.UserGroup.Id) && uug.UserGroup.RoleId == financeManagerBusinessRole.RoleId)
                )
                .Select(u => u.GdamUserId)
                .ToListAsync();
        }

        public async Task<string> GetApprover(Guid costId, Guid approverUserId, string approvalType)
        {
            var cost = await _efContext.Cost
                .FirstOrDefaultAsync(c => c.Id == costId);
            string approver;
            if (cost.IsExternalPurchases && approvalType == core.Constants.EmailApprovalType.Brand)
            {
                // get the Io Number Owner
                approver = await GetIoNumberOwner(cost.LatestCostStageRevisionId.Value);
            }
            else
            {
                approver = await GetCostUser(approverUserId);
            }

            return approver;
        }

        private async Task<List<string>> ReturnAllUserGdamIds(string businessRoleKey, string businessRoleType = core.Constants.AccessObjectType.Client)
        {
            return await _efContext.CostUser
                .Include(cu => cu.UserBusinessRoles)
                .ThenInclude(ubr => ubr.BusinessRole)
                .Where(u => u.UserBusinessRoles.Any(ubr => 
                    ubr.BusinessRole.Key == businessRoleKey && 
                    (ubr.ObjectType == businessRoleType || ubr.ObjectType == core.Constants.AccessObjectType.Client)))
                .Select(cu => cu.GdamUserId)
                .ToListAsync();
        }

        private async Task<string> GetCostUser(Guid userId)
        {
            return (await _efContext.CostUser
                .FirstOrDefaultAsync(u => u.Id == userId)).FullName;
        }

        private async Task<string> GetIoNumberOwner(Guid costStageRevisionId)
        {
            var pgPurchaseOrder = await _customObjectDataService.GetCustomData<PgPaymentDetails>(costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails);
            return pgPurchaseOrder.IoNumberOwner;
        }
    }
}

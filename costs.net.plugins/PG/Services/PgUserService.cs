namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.Response;
    using core.Events.Agency;
    using core.Events.CostUser;
    using core.Extensions;
    using core.Models;
    using core.Models.ActivityLog;
    using core.Models.ACL;
    using core.Models.Costs;
    using core.Models.Response;
    using core.Models.User;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.User;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Serilog;

    public class PgUserService : IPgUserService
    {
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly EFContext _efContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IActivityLogService _activityLogService;
        private readonly IEventService _eventService;
        private readonly IPluginAgencyService _pgAgencyService;

        private static readonly string[] CreateCostBusinessRoles =
        {
            Constants.BusinessRole.AgencyOwner,
            Constants.BusinessRole.AgencyAdmin,
            Constants.BusinessRole.CentralAdaptationSupplier,
        };

        private static readonly string[] ManageUsersBusinessRoles =
        {
            Constants.BusinessRole.AgencyAdmin,
            Constants.BusinessRole.GovernanceManager,
            Constants.BusinessRole.AdstreamAdmin
        };

        public PgUserService(
            EFContext efContext,
            IMapper mapper,
            IPermissionService permissionService,
            ILogger logger,
            ICostStageRevisionService costStageRevisionService,
            IActivityLogService activityLogService,
            IEventService eventService,
            IPluginAgencyService pgAgencyService)
        {
            _efContext = efContext;
            _mapper = mapper;
            _permissionService = permissionService;
            _logger = logger;
            _costStageRevisionService = costStageRevisionService;
            _activityLogService = activityLogService;
            _eventService = eventService;
            _pgAgencyService = pgAgencyService;
        }

        public async Task<OperationResponse> UpdateUser(UserIdentity userIdentity, Guid id, UpdateUserModel updateUserModel, BuType buType)
        {
            var xUserId = userIdentity.Id;
            var dbUser = _efContext.CostUser
                .Include(cu => cu.Agency)
                    .ThenInclude(a => a.GlobalAgencyRegion)
                        .ThenInclude(gar => gar.GlobalAgency)
                .Include(a => a.UserBusinessRoles)
                .ThenInclude(ubr => ubr.BusinessRole)
                .Include(a => a.UserUserGroups)
                .ThenInclude(uug => uug.UserGroup)
                .SingleOrDefault(a => a.Id == id);

            var userPerformingAction = _efContext.CostUser
                .Include(cu => cu.Agency)
                .Include(a => a.UserBusinessRoles)
                    .ThenInclude(ubr => ubr.BusinessRole)
                        .ThenInclude(br => br.Role)
                .SingleOrDefault(a => a.Id == xUserId);

            var businessRoles = _efContext.BusinessRole.ToList();

            if (dbUser == null)
            {
                return new OperationResponse(false, "User doesn't exist.");
            }

            var hasAccess = await _permissionService.CheckHasAccess(xUserId, id, AclActionType.Edit, "user");
            if (!hasAccess)
            {
                hasAccess = userPerformingAction.Agency.Id == dbUser.Agency.Id
                            && userPerformingAction.UserBusinessRoles.Any(ubr => ubr.BusinessRole.Key == Constants.BusinessRole.AgencyAdmin);
            }

            SetMissingObjectId(updateUserModel.AccessDetails, businessRoles, out var abstractType);
            if (updateUserModel.AccessDetails.Count > 0 && dbUser.UserBusinessRoles.Count > 0)
            {
                updateUserModel.AccessDetails = GetAllAccessDetails(updateUserModel.AccessDetails, dbUser.UserBusinessRoles);
            }
            //This is done because if you dont it will keep its reference and cause issues lower in this function
            var toBeRemoved = dbUser.UserBusinessRoles
                .Where(ubr =>
                    !updateUserModel.AccessDetails.Any(ad =>
                    ubr.BusinessRoleId == ad.BusinessRoleId
                    && ubr.ObjectType == ad.ObjectType
                    && (ubr.ObjectId == ad.ObjectId
                        || ubr.ObjectId == ad.OriginalObjectId
                        || ubr.Labels.Contains(ad.LabelName)
                        )
                    || ad.ObjectType == core.Constants.AccessObjectType.Client && ubr.ObjectType == ad.ObjectType && ubr.BusinessRoleId == ad.BusinessRoleId
                    )
                )
                .ToList();

            var entries = new List<IActivityLogEntry>();

            await _efContext.InTransactionAsync(async () =>
            {
                await RemoveUserFromExistingCosts(toBeRemoved, dbUser);
                await UpdateBusinessRoleLabels(updateUserModel.AccessDetails, dbUser);

                var agenciesToUpdateInElastic = new List<AbstractType>();
                //Add New Access Rules
                foreach (var accessDetail in updateUserModel.AccessDetails)
                {
                    if (!hasAccess)
                    {
                        hasAccess = userPerformingAction.UserBusinessRoles.Any(ubr =>
                            new[]
                            {
                                Roles.PlatformOwner,
                                Roles.ClientAdmin,
                                Roles.AdstreamAdmin
                            }.Contains(ubr.BusinessRole.Role.Name)
                            && ubr.ObjectId == accessDetail.ObjectId
                            && ubr.CostUserId == xUserId
                        );

                        if (!hasAccess)
                        {
                            // TODO: show the user which rules have been skipped
                            _logger.Warning(
                                $"User {userPerformingAction.Email}|{userPerformingAction.Id} does not have access to alter roles for {dbUser.Email}|{dbUser.Id} in scope of {accessDetail.ObjectType}|{accessDetail.ObjectId}, skipping this rule!");
                            continue;
                        }
                    }

                    var selectedBusinessRole = businessRoles.SingleOrDefault(a => a.Id == accessDetail.BusinessRoleId);

                    if (selectedBusinessRole == null)
                    {
                        throw new Exception($"Couldn't find business role with id {accessDetail.BusinessRoleId}");
                    }

                    if (dbUser.UserBusinessRoles.Any(userBusinessRole =>
                        userBusinessRole != null
                        && userBusinessRole.BusinessRole.Id == accessDetail.BusinessRoleId
                        && userBusinessRole.ObjectType == accessDetail.ObjectType
                        && (string.IsNullOrEmpty(accessDetail.LabelName) || userBusinessRole.Labels.Contains(accessDetail.LabelName))))
                    {
                        //Break out if user already has access.
                        _logger.Information(
                            $"User {dbUser.Id} already has access to {accessDetail.ObjectType} | {accessDetail.ObjectId} with BusinessRole {selectedBusinessRole.Key}|{selectedBusinessRole.Id}");
                        continue;
                    }

                    if (accessDetail.ObjectType == core.Constants.AccessObjectType.Smo
                    || accessDetail.ObjectType == core.Constants.AccessObjectType.Region
                        && selectedBusinessRole.Key != Constants.BusinessRole.RegionalAgencyUser)
                    {
                        _logger.Information($"User {dbUser.Email}|{dbUser.Id} will be given access at cost creation.");
                        var userBusinessRole = dbUser.UserBusinessRoles.FirstOrDefault(ubr =>
                            ubr.ObjectType == accessDetail.ObjectType);
                        if (userBusinessRole != null)
                        {
                            var labels = userBusinessRole.Labels.ToList();
                            labels.Add(accessDetail.LabelName);
                            userBusinessRole.Labels = labels.Distinct().ToArray();
                        }
                        else
                        {
                            userBusinessRole = new UserBusinessRole(xUserId)
                            {
                                BusinessRole = selectedBusinessRole,
                                Labels = new[] { accessDetail.LabelName },
                                ObjectType = accessDetail.ObjectType
                            };
                            dbUser.UserBusinessRoles.Add(userBusinessRole);
                            entries.Add(new UserRoleAssigned(dbUser.Email, selectedBusinessRole.Key, userIdentity));
                            await AddUserToExistingCosts(dbUser, userBusinessRole);
                        }
                        continue;
                    }

                    if (accessDetail.ObjectType == core.Constants.AccessObjectType.Region
                        && selectedBusinessRole.Key == Constants.BusinessRole.RegionalAgencyUser)
                    {
                        _logger.Information($"User {dbUser.Email}|{dbUser.Id} will be given access at cost creation.");
                        var userBusinessRole = dbUser.UserBusinessRoles.FirstOrDefault(ubr =>
                            ubr.ObjectType == accessDetail.ObjectType);
                        if (userBusinessRole != null)
                        {
                            var labelIds = userBusinessRole.LocationIds.ToList();
                            labelIds.Add(accessDetail.LabelId.ToString());
                            userBusinessRole.LocationIds = labelIds.Distinct().ToArray();

                            var labels = userBusinessRole.Labels.ToList();
                            labels.Add(accessDetail.LabelName);
                            userBusinessRole.Labels = labels.Distinct().ToArray();
                        }
                        else
                        {
                            userBusinessRole = new UserBusinessRole(xUserId)
                            {
                                BusinessRole = selectedBusinessRole,
                                Labels = new[] { accessDetail.LabelName },
                                LocationIds = new[] { accessDetail.LabelId?.ToString() },
                                ObjectType = accessDetail.ObjectType
                            };
                            dbUser.UserBusinessRoles.Add(userBusinessRole);
                            entries.Add(new UserRoleAssigned(dbUser.Email, selectedBusinessRole.Key, userIdentity));
                            await AddUserToExistingCosts(dbUser, userBusinessRole);
                        }
                        continue;
                    }
                    
                    if (selectedBusinessRole.Key == Constants.BusinessRole.AdstreamAdmin
                    || dbUser.Agency.Labels.Contains(Constants.Agency.PgOwnerLabel)
                    || dbUser.Agency.Labels.Contains(Constants.Agency.AdstreamOwnerLabel)
                    || (abstractType.Module != null && abstractType.Module?.ClientType == ClientType.Root))
                    {
                        abstractType = await _efContext.AbstractType.FirstOrDefaultAsync(at => at.Id == accessDetail.ObjectId);
                        var labels = abstractType.Module?.ClientType == ClientType.Root ? new[] { ClientType.Root.ToString() } : new[] { core.Constants.AccessObjectType.Client };
                        await GrantAccessToAbstractType(new [] { abstractType }, xUserId, buType, dbUser, selectedBusinessRole, accessDetail, labels);

                        entries.Add(new UserRoleAssigned(dbUser.Email, selectedBusinessRole.Key, userIdentity));
                        agenciesToUpdateInElastic.Add(abstractType);
                    }
                    else if (selectedBusinessRole.Key == Constants.BusinessRole.CostConsultant)
                    {
                        // If business role is "Cost Consultant" the user doesn't get access to any object at this stage.
                        dbUser.UserBusinessRoles.Add(new UserBusinessRole(xUserId)
                        {
                            BusinessRole = selectedBusinessRole,
                            ObjectType = accessDetail.ObjectType
                        });
                        entries.Add(new UserRoleAssigned(dbUser.Email, selectedBusinessRole.Key, userIdentity));
                    }
                    else
                    {
                        var agencyToCreatePseudoAgencies = new[] { dbUser.Agency };
                        if (selectedBusinessRole.Key == Constants.BusinessRole.RegionalAgencyUser)
                        {    
                            // TODO: use id of GlobalAgencyRegion in accessDetail rather then label because different Agencies can have regions with the same name. The same for Budget region. 
                            agencyToCreatePseudoAgencies = await _efContext.GlobalAgencyRegion
                                .Where(gar =>
                                    gar.Region == accessDetail.LabelName
                                    && gar.GlobalAgencyId == dbUser.Agency.GlobalAgencyRegion.GlobalAgencyId)
                                .SelectMany(gar => gar.GlobalAgency.GlobalAgencyRegions)
                                .SelectMany(gar => gar.Agencies)
                                .ToArrayAsync();
                        }

                        var pseudoAgencies = await _pgAgencyService.GetOrCreatePseudoAgencies(agencyToCreatePseudoAgencies);
                        await GrantAccessToAbstractType(pseudoAgencies, xUserId, buType, dbUser, selectedBusinessRole, accessDetail);
                        agenciesToUpdateInElastic.AddRange(pseudoAgencies);
                    }
                }

                dbUser.ApprovalLimit = updateUserModel.ApprovalLimit;
                dbUser.EmailUrl = updateUserModel.EmailOverride;
                dbUser.NotificationBudgetRegionId = updateUserModel.NotificationBudgetRegionId;
                dbUser.UserGroups = await _permissionService.GetObjectUserGroups(dbUser.Id, null);

                await _activityLogService.LogRange(entries);

                _eventService.Add(new CostUsersUpdated(_mapper.Map<CostUserSearchItem[]>(new [] { dbUser })));
                if (agenciesToUpdateInElastic.Any())
                {
                    _eventService.Add(new AgenciesUpdated(_mapper.Map<AgencySearchItem[]>(agenciesToUpdateInElastic)));
                }

            }, async () => await _eventService.SendAllPendingAsync());

            return new OperationResponse(true, "User updated.");
        }

        public Task<bool> UserHasAccess(Guid costId, UserIdentity user)
        {
            return _permissionService.CheckHasAccess(user.Id, costId, AclActionType.Edit, nameof(Cost).ToLowerInvariant());
        }

        public async Task GrantUsersAccess(Cost cost, CreateCostModel model)
        {
            var usersToGrantAccessTo = await _efContext.CostUser
                .Include(user => user.UserBusinessRoles)
                    .ThenInclude(ubr => ubr.BusinessRole)
                .Where(user => user.UserBusinessRoles.Any(ubr =>
                    ubr.ObjectType == core.Constants.AccessObjectType.Region ||
                    ubr.ObjectType == core.Constants.AccessObjectType.Smo
                )).ToListAsync();

            var stageDetails = model.StageDetails.Data.ToModel<PgStageDetailsForm>();
            foreach (var user in usersToGrantAccessTo)
            {
                foreach (var userBusinessRole in user.UserBusinessRoles)
                {
                    switch (userBusinessRole.ObjectType)
                    {
                        case core.Constants.AccessObjectType.Region:
                            if (userBusinessRole.Labels != null && (
                                    userBusinessRole.Labels.Any(label => label == stageDetails?.BudgetRegion?.Name)
                                    ||
                                    userBusinessRole.BusinessRole.Key == Constants.BusinessRole.RegionalAgencyUser
                                    && cost.Owner.Agency.GlobalAgencyRegion != null
                                    && userBusinessRole.Labels.Contains(cost.Owner.Agency.GlobalAgencyRegion.Region)
                                ))
                            {
                                await _permissionService.GrantUserAccess<Cost>(userBusinessRole.BusinessRole.RoleId, cost.Id, user, BuType.Pg, null, null, false);
                            }
                            break;
                        case core.Constants.AccessObjectType.Smo:
                            if (userBusinessRole.Labels != null && !string.IsNullOrEmpty(stageDetails.SmoName) &&
                                userBusinessRole.Labels.Any(label => label == stageDetails.SmoName))
                            {
                                await _permissionService.GrantUserAccess<Cost>(userBusinessRole.BusinessRole.RoleId, cost.Id, user, BuType.Pg, null, null, false);
                            }
                            break;
                    }
                }
            }
        }

        public bool CanCreateCost(CostUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            return
                user.UserBusinessRoles != null
                && user.UserBusinessRoles.Any(ubr => ubr.BusinessRole != null && CreateCostBusinessRoles.Contains(ubr.BusinessRole.Key));
        }

        public void AggregateBusinessRoles(CostUser costUser)
        {
            costUser.UserBusinessRoles = costUser.UserBusinessRoles.GroupBy(ubr => new { ubr.ObjectType, ubr.BusinessRoleId }).Select(g => g.First()).ToList();
        }

        public bool CanManageUsers(CostUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }
            return
                user.UserBusinessRoles != null
                && user.UserBusinessRoles.Any(ubr => ubr.BusinessRole != null && ManageUsersBusinessRoles.Contains(ubr.BusinessRole.Key));
        }

        public bool IsUserAgencyAdmin(CostUser user)
        {
            return DoesUserHaveRole(user, Constants.BusinessRole.AgencyAdmin);
        }

        public bool IsUserAgencyOwner(CostUser user)
        {
            return DoesUserHaveRole(user, Constants.BusinessRole.AgencyOwner);
        }

        public async Task AddUsersToAgencyAbstractType(AbstractType agencyAbstractType, Guid userId)
        {
            var agencyUsers = _efContext.CostUser
                .Include(cu => cu.UserBusinessRoles)
                .ThenInclude(ubr => ubr.BusinessRole)
                .Where(a => a.AgencyId == agencyAbstractType.Agency.Id).ToList();

            foreach (var user in agencyUsers)
            {
                var clientRole = user.UserBusinessRoles
                    .Where(ubr => ubr.ObjectType == core.Constants.AccessObjectType.Client)
                    .GroupBy(ubr => new { ubr.ObjectType, ubr.BusinessRoleId })
                    .Select(g => g.First()).FirstOrDefault();

                if (clientRole == null)
                {
                    continue;
                }
                await _permissionService.GrantUserAccess<AbstractType>(clientRole.BusinessRole.RoleId, agencyAbstractType.Id, user, BuType.Pg, null, null, false);
                user.UserBusinessRoles.Add(new UserBusinessRole(userId)
                {
                    BusinessRole = clientRole.BusinessRole,
                    ObjectId = agencyAbstractType.Id,
                    ObjectType = core.Constants.AccessObjectType.Client,
                });
            }
        }

        public async Task AddUserToExistingCosts(CostUser user, UserBusinessRole userBusinessRole)
        {
            if (userBusinessRole.BusinessRole.Key == Constants.BusinessRole.RegionalAgencyUser && userBusinessRole.ObjectType == core.Constants.AccessObjectType.Region)
            {
                var costs = await _efContext.Cost
                    .Where(c => c.Owner.Agency.GlobalAgencyRegionId != null)
                    .Where(c => userBusinessRole.LocationIds.Contains(c.Owner.Agency.GlobalAgencyRegion.Id.ToString()))
                    .Select(c => new { costId = c.Id, agencyRegion = c.Owner.Agency.GlobalAgencyRegion.Region })
                    .ToListAsync();

                foreach (var cost in costs)
                {
                    await _permissionService.GrantUserAccess<Cost>(
                        userBusinessRole.BusinessRole.RoleId, cost.costId, user, BuType.Pg, null, cost.agencyRegion, false);
                }
            }
            else
            {
                var queryableCosts = GetQueryableCostsByUserObjectTypeAndLabels(userBusinessRole.ObjectType, userBusinessRole.Labels);
                if (queryableCosts == null)
                {
                    return;
                }

                var costs = await queryableCosts
                    .Include(c => c.LatestCostStageRevision)
                        .ThenInclude(csr => csr.StageDetails)
                    .ToArrayAsync();

                foreach (var cost in costs)
                {
                    var costStageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(cost.LatestCostStageRevision);
                    switch (userBusinessRole.ObjectType)
                    {
                        case core.Constants.AccessObjectType.Smo:
                            if (!user.UserUserGroups.Any() || !user.UserUserGroups.Any(uug => uug.UserGroup.Label == costStageDetails.SmoName && uug.UserGroup.ObjectId == cost.Id))
                            {
                                await _permissionService.GrantUserAccess<Cost>(
                                    userBusinessRole.BusinessRole.RoleId, cost.Id, user, BuType.Pg, null, costStageDetails.SmoName, false);
                            }
                            break;
                        case core.Constants.AccessObjectType.Region:
                            if (!user.UserUserGroups.Any() || user.UserUserGroups.Any(uug => uug.UserGroup.Label != costStageDetails.BudgetRegion.Name))
                            {
                                await _permissionService.GrantUserAccess<Cost>(
                                    userBusinessRole.BusinessRole.RoleId, cost.Id, user, BuType.Pg, null, costStageDetails.BudgetRegion.Name, false);
                            }
                            break;
                    }
                }
            }
        }

        public async Task RemoveUserFromExistingCosts(List<UserBusinessRole> businessRoles, CostUser user)
        {
            foreach (var br in businessRoles)
            {
                var userUserGroups = user.UserUserGroups
                    .Where(uug => 
                        uug.UserGroup.ObjectId == br.ObjectId 
                        || !string.IsNullOrEmpty(uug.UserGroup.Label) && br.Labels.Any(l => l == uug.UserGroup.Label)
                    ).ToList();

                foreach (var userUserGroup in userUserGroups)
                {
                    await _permissionService.RevokeAccessForSubjectWithRole(userUserGroup.UserGroup.ObjectId, user.Id, userUserGroup.UserGroup.RoleId, null, false);
                }

                user.UserBusinessRoles.Remove(br);
            }
        }

        private static bool DoesUserHaveRole(CostUser user, string businessRoleKey)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }
            return
                user.UserBusinessRoles != null
                && user.UserBusinessRoles.Any(ubr => ubr.BusinessRole != null && ubr.BusinessRole.Key == businessRoleKey);
        }

        /// <summary>
        /// Returns the list of updated/created pseudo agencies
        /// </summary>
        /// <param name="agencyAbstractTypes"></param>
        /// <param name="xUserId"></param>
        /// <param name="buType"></param>
        /// <param name="dbUser"></param>
        /// <param name="selectedBusinessRole"></param>
        /// <param name="accessDetail"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        private async Task GrantAccessToAbstractType(IEnumerable<AbstractType> agencyAbstractTypes, Guid xUserId, BuType buType, CostUser dbUser, BusinessRole selectedBusinessRole, AccessDetail accessDetail, string[] labels = null)
        {
            foreach (var agencyAbstractType in agencyAbstractTypes)
            {
                await _permissionService.GrantUserAccess<AbstractType>(selectedBusinessRole.RoleId, agencyAbstractType.Id, dbUser, buType, null, null, false);
                dbUser.UserBusinessRoles.Add(new UserBusinessRole(xUserId)
                {
                    BusinessRole = selectedBusinessRole,
                    ObjectId = agencyAbstractType.Id,
                    ObjectType = accessDetail.ObjectType,
                    Labels = labels ?? new string[0]
                });

                _logger.Information(
                    $"User {xUserId} granted access for object {agencyAbstractType.Id} of type Parent with BusinessRole {selectedBusinessRole.Key}|{selectedBusinessRole.Id}");
            }
        }

        private List<AccessDetail> GetAllAccessDetails(List<AccessDetail> accessDetails, List<UserBusinessRole> dbUserUserBusinessRoles)
        {
            var existingAccessDetails = new List<AccessDetail>();
            dbUserUserBusinessRoles.ForEach(ubr => accessDetails.ForEach(ad =>
            {
                if (ubr.BusinessRoleId == ad.BusinessRoleId && ubr.ObjectType == ad.ObjectType)
                {
                    existingAccessDetails.Add(_mapper.Map<AccessDetail>(ubr));
                }
            }));
            foreach (var ad in accessDetails)
            {
                existingAccessDetails.RemoveAll(a => a.ObjectId == ad.ObjectId || a.BusinessRoleId == ad.BusinessRoleId && ad.ObjectType == a.ObjectType);
                if (dbUserUserBusinessRoles.Any(a => a.Labels.Contains(ad.LabelName)))
                {
                    var selectedAccessDetail = _mapper.Map<AccessDetail>(dbUserUserBusinessRoles.First(a => a.Labels.Contains(ad.LabelName)));
                    var selectedAds = existingAccessDetails
                        .Where(a => a.BusinessRoleId == selectedAccessDetail.BusinessRoleId && a.ObjectType == selectedAccessDetail.ObjectType)
                        .ToList();

                    selectedAds.ForEach(a => existingAccessDetails.Remove(a));
                }
            }
            accessDetails.AddRange(existingAccessDetails);
            return accessDetails;
        }

        private void SetMissingObjectId(List<AccessDetail> accessDetails, List<BusinessRole> businessRoles, out AbstractType abstractType)
        {
            abstractType = _efContext.AbstractType
                .Include(a => a.Module)
                .FirstOrDefault(a =>
                    a.Type == core.Constants.AccessObjectType.Module
                    && a.Module.ClientType == ClientType.Root);

            var adstreamAdminBusinessRole = businessRoles.FirstOrDefault(a => a.Key == Constants.BusinessRole.AdstreamAdmin);
            if (adstreamAdminBusinessRole == null)
            {
                throw new Exception($"Couldn't find business role {Constants.BusinessRole.AdstreamAdmin}");
            }

            foreach (var accessDetail in accessDetails)
            {
                if (accessDetail.ObjectId != null)
                {
                    continue;
                }

                if (accessDetail.BusinessRoleId == adstreamAdminBusinessRole.Id)
                {
                    accessDetail.ObjectId = abstractType.Id;
                }
                else
                {
                    abstractType = _efContext.AbstractType
                        .Include(at => at.Module)
                        .First(at =>
                            at.Type == core.Constants.AccessObjectType.Module
                            && at.Module.ClientType == ClientType.Pg);

                    accessDetail.ObjectId = abstractType.Id;
                }
            }
        }

        private async Task UpdateBusinessRoleLabels(IReadOnlyCollection<AccessDetail> accessDetails, CostUser dbUser)
        {
            var regionalBusinessRole = dbUser.UserBusinessRoles.FirstOrDefault(a => a.ObjectType == core.Constants.AccessObjectType.Region);
            if (regionalBusinessRole != null)
            {
                var regionLabels = accessDetails.Where(a => a.ObjectType == core.Constants.AccessObjectType.Region && a.LabelName != null).Select(a => a.LabelName).ToArray();
                var regionLabelIds = accessDetails.Where(a => a.ObjectType == core.Constants.AccessObjectType.Region && a.LabelId != null).Select(a => a.LabelId.ToString()).ToArray();

                var added = regionLabels.Except(regionalBusinessRole.Labels).ToList();
                var removed = regionalBusinessRole.Labels.Except(regionLabels).ToList();

                if (removed.Any())
                {
                    await RemoveUserFromExistingCosts(removed, regionalBusinessRole, dbUser);
                    regionalBusinessRole.Labels = regionLabels;
                    regionalBusinessRole.LocationIds = regionLabelIds;
                }
                if (added.Any())
                {
                    regionalBusinessRole.Labels = regionLabels;
                    regionalBusinessRole.LocationIds = regionLabelIds;
                    await AddUserToExistingCosts(dbUser, regionalBusinessRole);
                }
            }

            var smoBusinessRole = dbUser.UserBusinessRoles.FirstOrDefault(a => a.ObjectType == core.Constants.AccessObjectType.Smo);
            if (smoBusinessRole != null)
            {
                var smoLabels = accessDetails.Where(a => a.ObjectType == core.Constants.AccessObjectType.Smo).Select(a => a.LabelName).ToArray();

                var added = smoLabels.Except(smoBusinessRole.Labels).ToList();
                var removed = smoBusinessRole.Labels.Except(smoLabels).ToList();

                if (removed.Any())
                {
                    await RemoveUserFromExistingCosts(removed, smoBusinessRole, dbUser);
                    smoBusinessRole.Labels = smoLabels;
                }
                if (added.Any())
                {
                    smoBusinessRole.Labels = smoLabels;
                    await AddUserToExistingCosts(dbUser, smoBusinessRole);
                }
            }
        }

        private IQueryable<Cost> GetQueryableCostsByUserObjectTypeAndLabels(string objectType, IEnumerable<string> labels)
        {
            string[] stageDetailsFieldName;
            switch (objectType)
            {
                case core.Constants.AccessObjectType.Smo:
                    stageDetailsFieldName = new []{ nameof(PgStageDetailsForm.SmoName) };
                    break;
                // TODO: path key rather than name in the label from UI and change here nameof(AbstractTypeValue.Name) to nameof(AbstractTypeValue.Key)
                case core.Constants.AccessObjectType.Region:
                    stageDetailsFieldName = new [] { nameof(PgStageDetailsForm.BudgetRegion), nameof(AbstractTypeValue.Name) };
                    break;
                default:
                    return null;
            }

            var labelsArray = labels as string[] ?? labels.ToArray();
            return _efContext.GetCostsByStageDetailsFieldValue(stageDetailsFieldName, labelsArray);
        }

        private async Task RemoveUserFromExistingCosts(IList<string> removedLabels, UserBusinessRole userBusinessRole, CostUser user)
        {
            var costs = await GetQueryableCostsByUserObjectTypeAndLabels(userBusinessRole.ObjectType, removedLabels).ToListAsync();

            foreach (var cost in costs)
            {
                await _permissionService.RevokeAccessForSubjectWithRole(cost.Id, user.Id, userBusinessRole.BusinessRole.RoleId, null, false);
            }
        }
    }
}
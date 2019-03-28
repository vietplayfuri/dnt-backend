namespace costs.net.tests.common.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Extensions;
    using core.Models;
    using core.Models.ACL;
    using core.Models.Response;
    using core.Models.User;
    using core.Services;
    using dataAccess.Entity;

    public class PermissionServiceStub : IPermissionService
    {
        private readonly dataAccess.EFContext _efContext;

        public PermissionServiceStub(dataAccess.EFContext efContext)
        {
            _efContext = efContext;
        }

        public Task CheckAccess(Guid userId, Guid objectId, string action, string subType = null)
        {
            return Task.CompletedTask;
        }

        public Task<string[]> CreateDomainNode(string subType, Guid parentId, Guid externalId, Guid userId)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<string[]> AddUser(CostUser newUser)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public async Task<GrantAccessResponse> GrantUserAccess<T>(
            Guid roleId, Guid objectId, CostUser costUser, BuType buType, Guid? xUserId, string labels = null, bool saveChanges = true)
            where T : Entity, IUserGroup
        {
            var dbUser = await _efContext.CostUser.FindAsync(costUser.Id);
            var existingUserGroup = _efContext.UserGroup.FirstOrDefault(a => a.ObjectId == objectId && a.RoleId == roleId);
            if (existingUserGroup == null)
            {
                existingUserGroup = new UserGroup
                {
                    Id = Guid.NewGuid(),
                    ObjectId = objectId,
                    RoleId = roleId,
                    ObjectType = typeof(T).Name.ToSnakeCase(),
                    Label = labels
                };
                existingUserGroup.SetName();
            }
            _efContext.UserUserGroup.Add(new UserUserGroup
            {
                UserGroupId = existingUserGroup.Id,
                UserId = costUser.Id,
                UserGroup = existingUserGroup
            });

            dbUser.UserGroups = new[] { existingUserGroup.Id.ToString() };

            if (saveChanges)
            {
                await _efContext.SaveChangesAsync();
            }

            return new GrantAccessResponse { UserGroups = new[] { existingUserGroup.Id.ToString() }, New = true, UserGroup = existingUserGroup };

        }

        public Task<string[]> GetObjectUserGroups(Guid objectId, Guid? userId)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<string[]> GetSubjectUserGroups(Guid userId)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<string[]> RevokeAccessForSubject(Guid objectId, Guid userId, Guid? xUserId)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<string[]> RevokeAccessForSubjectWithRole(Guid objectId, Guid userId, Guid roleId, Guid? xUserId, bool saveChanges = true)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<bool> CheckHasAccess(Guid userId, Guid objectId, string action, string subType = null)
        {
            return Task.FromResult(true);
        }

        public Task RemoveDomainNode(Guid userId, Guid externalId)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<PermissionModel>> GetPermissions(Guid userId, Guid objectId)
        {
            return Task.FromResult(Enumerable.Empty<PermissionModel>());
        }

        public Task<IEnumerable<PermissionModel>> GetAllPermissions(UserIdentity user)
        {
            return Task.FromResult(new PermissionModel[] { }.AsEnumerable());
        }

        public Task<PermissionModel> GetPermissionByName(string name)
        {
            return Task.FromResult(new PermissionModel
            {
                Name = name
            });
        }

        public Task<string[]> GetUserGroupsWithPermission(Guid userId, string subType, string action)
        {
            return Task.FromResult(new[] { Guid.NewGuid().ToString() });
        }

        public Task<GrantAccessResponse> GrantAccess<T>(Guid roleId, Guid objectId, CostUser costUser, Guid? xUserId, bool saveChanges, string label = null) where T : Entity, IUserGroup
        {
            return Task.FromResult(new GrantAccessResponse { });
        }
    }
}

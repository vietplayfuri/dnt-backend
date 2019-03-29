namespace dnt.core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dnt.dataAccess.Entity;
    using Models;
    using Models.Response;
    using Models.User;

    public interface IPermissionService
    {
        Task CheckAccess(Guid userId, Guid objectId, string action, string subType = null);
        Task<bool> CheckHasAccess(Guid userId, Guid objectId, string action, string subType = null);
    }
}

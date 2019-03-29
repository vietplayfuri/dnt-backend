namespace dnt.core.Services.Role
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dnt.core.Models.Role;
    using dnt.dataAccess.Entity;
    using Models;

    public interface IRoleService
    {
        Task<List<RoleModel>> Get();
        Task<RoleModel> Create(RoleModel insertModel);
        Task<RoleModel> Update(RoleModel insertModel);
    }
}

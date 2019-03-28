namespace dnt.core.Services.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess;
    using dnt.dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class RoleService : IRoleService
    {
        private readonly EFContext _efContext;

        public RoleService(EFContext efContext)
        {
            _efContext = efContext;
        }

        public Task<List<Role>> GetRoles()
        {
            return _efContext.Role.ToListAsync();
        }
    }
}

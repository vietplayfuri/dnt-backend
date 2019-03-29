namespace dnt.core.Services.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using dataAccess;
    using dnt.core.Models.Role;
    using dnt.dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class RoleService : IRoleService
    {
        private readonly EFContext _efContext;
        private readonly IMapper _mapper;

        public RoleService(EFContext efContext, IMapper mapper)
        {
            _mapper = mapper;
            _efContext = efContext;
        }

        /// <summary>
        /// Get users
        /// </summary>
        public async Task<List<RoleModel>> Get()
        {
            return _mapper.Map<List<RoleModel>>(await _efContext.Role.OrderBy(u => u.Name).ToListAsync());
        }

        /// <summary>
        /// Create User
        /// </summary>
        public async Task<RoleModel> Create(RoleModel insertModel)
        {
            var role = _mapper.Map<Role>(insertModel);
            await _efContext.Role.AddAsync(role);
            await _efContext.SaveChangesAsync();

            return insertModel;
        }

        /// <summary>
        /// Update User
        /// </summary>
        public async Task<RoleModel> Update(RoleModel insertModel)
        {
            var role = _mapper.Map<Role>(insertModel);
            _efContext.Role.Update(role);
            await _efContext.SaveChangesAsync();

            return _mapper.Map<RoleModel>(role);
        }
    }
}

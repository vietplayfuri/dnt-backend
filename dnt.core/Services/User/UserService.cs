namespace dnt.core.Services.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using dnt.dataAccess.Entity;
    using dataAccess.Extensions;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Models;
    using Models.Response;
    using Models.User;
    using Models.Utils;
    using Serilog;
    using dnt.dataAccess;

    public class UserService : IUserService
    {
        private readonly EFContext _efContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public UserService(
            IMapper mapper,
            IPermissionService permissionService,
            ILogger logger,
            EFContext efContext)
        {
            _mapper = mapper;
            _logger = logger;
            _efContext = efContext;
        }


        /// <summary>
        /// Get users
        /// </summary>
        public async Task<bool> IsCorrectPassword(string username, string password)
        {
            var user = await _efContext.User.FirstOrDefaultAsync(u => u.Username == username && u.Disabled == false);
            if (user == null)
                return false;

            return password.HashPassword() == user.Password;
        }

        /// <summary>
        /// Get users
        /// </summary>
        public async Task<List<UserModel>> Get()
        {
            return _mapper.Map<List<UserModel>>(await _efContext.User.OrderBy(u => u.Fullname).ToListAsync());
        }

        /// <summary>
        /// Get user
        /// </summary>
        public async Task<UserModel> Get(long userId)
        {
            return _mapper.Map<UserModel>(await _efContext.User.FirstOrDefaultAsync(u => u.Id == userId));
        }

        /// <summary>
        /// Get user
        /// </summary>
        public async Task<UserModel> Get(string username)
        {
            return _mapper.Map<UserModel>(await _efContext.User.FirstOrDefaultAsync(u => u.Username == username));
        }

        /// <summary>
        /// Create User
        /// </summary>
        public async Task<UserModel> Create(UserInsertModel userInsertModel)
        {
            userInsertModel.Password = userInsertModel.Password.HashPassword();
            var user = _mapper.Map<User>(userInsertModel);
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = userInsertModel.RoleId
            });
            await _efContext.User.AddAsync(user);
            await _efContext.SaveChangesAsync();

            return _mapper.Map<UserModel>(user);
        }


        /// <summary>
        /// Update User
        /// </summary>
        public async Task<UserModel> Update(UserInsertModel userInsertModel)
        {
            userInsertModel.Password = userInsertModel.Password.HashPassword();
            var user = _mapper.Map<User>(userInsertModel);
            user.UserRoles.First().RoleId = userInsertModel.RoleId;

            _efContext.User.Update(user);
            await _efContext.SaveChangesAsync();

            return _mapper.Map<UserModel>(user);
        }
    }
}

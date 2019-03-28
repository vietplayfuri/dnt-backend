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
        private readonly AppSettings _appSettings;
        private const string Mdo = "MDO";
        private readonly EFContext _efContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public UserService(
            IMapper mapper,
            IPermissionService permissionService,
            ILogger logger,
            IOptions<AppSettings> appSettings,
            EFContext efContext)
        {
            _mapper = mapper;
            _permissionService = permissionService;
            _logger = logger;
            _efContext = efContext;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        ///     Get cost user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewMode"></param>
        /// <param name="buType"></param>
        /// <returns></returns>
        public async Task<ICostUserModel> Get(Guid id, UserViewMode? viewMode, BuType buType)
        {
            //var costUser = await GetCostUsersQueryable().FirstOrDefaultAsync(a => a.Id == id);
            //return Map(costUser, viewMode, buType);
            return null;
        }
    }
}

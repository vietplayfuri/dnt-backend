namespace dnt.core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using dnt.dataAccess.Entity;
    using dataAccess.Extensions;
    using Extensions;
    using Exceptions;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using Microsoft.Extensions.Options;
    using Models;
    using Models.Response;
    using Models.User;
    using Models.Utils;
    using User;
    using dnt.dataAccess;

    public class PermissionService : IPermissionService
    {
        private readonly AppSettings _appSettings;
        private readonly EFContext _efContext;
        private readonly ILogger _logger;

        public PermissionService(
            IOptions<AppSettings> appSettings,
            EFContext efContext,
            ILogger logger)
        {
            _efContext = efContext;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task CheckAccess(Guid userId, Guid objectId, string action, string subType = null)
        {
            var result = string.IsNullOrEmpty(subType)
                ? await CheckHasAccess(userId, objectId, action)
                : await CheckHasAccess(userId, objectId, action, subType);
            if (result == false)
            {
                throw new HttpException(HttpStatusCode.Unauthorized, $"User doesn't have permission to {action} item.");
            }
        }

        public async Task<bool> CheckHasAccess(Guid userId, Guid objectId, string action, string subType = null)
        {
            return true;
        }
    }
}

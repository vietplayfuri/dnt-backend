namespace dnt.api.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AutoMapper;
    using dnt.core.Models.Response;
    using dnt.core.Models.User;
    using dnt.dataAccess.Entity;
    using dataAccess;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Security.Authorization;
    using Serilog;
    using dnt.dataAccess;

    public class AuthMiddleware
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next, IMapper mapper)
        {
            _next = next;
            _mapper = mapper;
            _logger = Log.ForContext<AuthMiddleware>();
        }

        public async Task Invoke(HttpContext context, EFContext efContext)
        {
            if (RouteWhitelisted(context))
            {
                await _next(context);
            }
            else
            {
                UserIdentity ui = null;

                var gdamUserIdStr = context.Request.Query["$id$"].ToString();

                _logger.Debug($"Req: {context.Request.Path} GdamUserId: {gdamUserIdStr}");

                try
                {
                    ui = await GetUserByGdamUserId(100, efContext);
                }
                catch (Exception e) when (e is UnauthorizedAccessException)
                {
                    _logger.Error(e.Message);
                    await ReturnUnauthorised(e.Message, context);
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                    _logger.Debug("$id$ present in URL but user not found", e);
                }

                if (ui != null)
                {
                    context.Items.Add("user", ui);
                    context.User = GetClaimsPrincipal(ui);
                    await _next(context);
                }
                else
                {
                    _logger.Information($"{gdamUserIdStr} attempted to make {context.Request.QueryString} request but is not authorized!");
                    await ReturnUnauthorised("Unauthorized!", context);
                }
            }
        }

        private async Task ReturnUnauthorised(string message, HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var response = JsonConvert.SerializeObject(new ErrorResponse(message));
            //This doesnt execute the command but rather passes it to the next middleware (logger) for logging!
            await _next(context);
            await context.Response.WriteAsync(response);
        }

        private static bool RouteWhitelisted(HttpContext context)
        {
            // ** indicates a match all, e.g. /swagger** means the path must begin exactly with /swagger
            var whitelist = new List<string>
            {
                "/favicon.ico",
                "/v1/admin/version",
                "/status",
                "/swagger**"
            };

            return whitelist.Any(x =>
            {
                if (x == context.Request.Path)
                {
                    return true;
                }

                if (x.IndexOf("**", StringComparison.Ordinal) > -1)
                {
                    return context.Request.Path.StartsWithSegments(new PathString(x.Replace("**", string.Empty)), StringComparison.OrdinalIgnoreCase);
                }

                return false;
            });
        }

        private async Task<UserIdentity> GetUserByGdamUserId(long gdamUserId, EFContext efContext)
        {
            var costUser = await efContext.User
                .Select(cu => new User
                {
                    Id = cu.Id,
                    Email = cu.Email,
                    Fullname = cu.Fullname,
                    Disabled = cu.Disabled
                })
                .FirstOrDefaultAsync(u => u.Id == gdamUserId);

            if (costUser == null)
            {
                return null;
            }

            if (costUser.Disabled)
            {
                throw new UnauthorizedAccessException("User is disabled, please contact your system admin to look into this for you!");
            }

            var userIdentity = _mapper.Map<UserIdentity>(costUser);
            return userIdentity;
        }

        private static ClaimsPrincipal GetClaimsPrincipal(UserIdentity ui)
        {
            var claims = new List<Claim>
            {
                new Claim(CostClaimTypes.GdamId, ui.GdamUserId)
            };
            var claimsIdentity = new CostsClaimsIdentity(claims, ui.Email);
            var userPrincipal = new ClaimsPrincipal(claimsIdentity);

            return userPrincipal;
        }
    }
}

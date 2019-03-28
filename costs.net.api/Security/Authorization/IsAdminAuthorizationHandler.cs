namespace dnt.api.Security.Authorization
{
    using System.Threading.Tasks;
    using dnt.core.Models.Utils;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Options;

    public sealed class IsAdminAuthorizationHandler : AuthorizationHandler<IsAdminRequirement>
    {
        private readonly AppSettings _appSettings;

        public IsAdminAuthorizationHandler(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == CostClaimTypes.GdamId && c.Value == _appSettings.AdminUser))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
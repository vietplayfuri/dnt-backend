namespace dnt.api.Security.Authorization
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    public static class AuthorizationConfigurationExtension
    {
        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
                options.AddPolicy(
                    PolicyNames.Admin,
                    b => b.Requirements.Add(new IsAdminRequirement())
                )
            );
            services.AddSingleton<IAuthorizationHandler, IsAdminAuthorizationHandler>();
        }
    }
}
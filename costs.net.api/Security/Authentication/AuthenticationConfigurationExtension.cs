namespace dnt.api.Security.Authentication
{
    using Microsoft.Extensions.DependencyInjection;

    public static class AuthenticationConfigurationExtension
    {
        private const string ForbidSchema = "Forbid";

        public static void AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthenticationCore(options =>
            {
                options.DefaultForbidScheme = ForbidSchema;
                options.AddScheme<ForbidSchemeHandler>(ForbidSchema, ForbidSchema);
            });
        }
    }
}
namespace dnt.api.Security.Authentication
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public sealed class ForbidSchemeHandler : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return Task.FromResult(null as AuthenticateResult);
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }
    }
}
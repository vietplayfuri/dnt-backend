namespace dnt.api.Extensions
{
    using dnt.core.Models.User;
    using dnt.dataAccess.Entity;
    using Microsoft.AspNetCore.Http;

    public class RequestContext : IRequestContext
    {
        public HttpContext Context { get; set; }

        public UserIdentity User => (UserIdentity) Context?.Items["user"];

        public void SetContext(HttpContext context)
        {
            Context = context;
        }
    }
}

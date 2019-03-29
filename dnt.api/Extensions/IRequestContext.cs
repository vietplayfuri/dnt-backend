namespace dnt.api.Extensions
{
    using dnt.core.Models.User;
    using dnt.dataAccess.Entity;
    using Microsoft.AspNetCore.Http;

    public interface IRequestContext
    {
        HttpContext Context { get; set; }

        UserIdentity User { get; }

        void SetContext(HttpContext context);
    }
}

namespace dnt.api.Extensions
{
    using dnt.core.Models.User;
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerExtensions
    {
        public static UserIdentity GetUser(this Controller controller)
        {
            return (UserIdentity) controller.Request.HttpContext.Items["user"];
        }
    }
}

namespace dnt.api.Controllers
{
    using dnt.core.Models.User;
    using dnt.core.Services;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public abstract class CostsControllerBase : Controller
    {
        protected CostsControllerBase(IPermissionService permissionService)
        {
            PermissionService = permissionService;
        }

        protected IPermissionService PermissionService { get; }

        protected new UserIdentity User { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //User = this.GetUser();
            //User.IpAddress = context.HttpContext.Request.Headers["X-Forwarded-For"];
            base.OnActionExecuting(context);
        }
    }
}

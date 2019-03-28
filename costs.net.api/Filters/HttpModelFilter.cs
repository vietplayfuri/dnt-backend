
namespace dnt.api.Filters
{
    using System.Collections.Generic;
    using dnt.core.Models.Web;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class HttpModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                foreach (KeyValuePair<string, object> args in context.ActionArguments)
                {
                    if (args.Value is HttpModel)
                    {
                        var model = args.Value as HttpModel;
                        model.IpAddress = context.HttpContext.Request.Headers["X-Forwarded-For"];
                    }
                }
            }
        }
    }
}

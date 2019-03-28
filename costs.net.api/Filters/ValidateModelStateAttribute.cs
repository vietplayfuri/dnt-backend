using dnt.api.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;

namespace dnt.api.Filters
{
    using dnt.core.Models.Response;

    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var response = new ErrorResponse(errors);

                context.Result = response.ToActionResult(HttpStatusCode.BadRequest);
            }
        }
    }
}

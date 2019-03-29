using Microsoft.AspNetCore.Mvc;

namespace dnt.api.Extensions
{
    using dnt.core.Models.Response;

    public static class OperationResponseExtensions
    {
        public static IActionResult ToActionResult(this OperationResponse response)
        {
          return response.Success
              ? (IActionResult) new OkObjectResult(response.Object ?? response) 
              : new BadRequestObjectResult(response);
        }
    }
}

namespace dnt.api.Extensions
{
    using dnt.core.Models.Response;
    using Microsoft.AspNetCore.Mvc;

    public static class ElasticBootstrapResultExtension
    {
        public static IActionResult ToActionResult(this ElasticBootstrapResponse response)
        {
            return response.Error ? (IActionResult) new BadRequestObjectResult(response) : new OkObjectResult(response);
        }
    }
}

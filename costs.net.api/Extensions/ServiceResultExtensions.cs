namespace dnt.api.Extensions
{
    using dnt.core.Services;
    using Microsoft.AspNetCore.Mvc;

    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult(this ServiceResult serviceResult)
        {
            return serviceResult.Success 
                ? (IActionResult) new OkObjectResult(serviceResult) 
                : new BadRequestObjectResult(serviceResult);
        }

        public static IActionResult ToActionResult<T>(this ServiceResult<T> serviceResult)
            where T : class, new()
        {
            return serviceResult.Success 
                ? (IActionResult) new OkObjectResult(serviceResult)
                : new BadRequestObjectResult(serviceResult);
        }
    }
}
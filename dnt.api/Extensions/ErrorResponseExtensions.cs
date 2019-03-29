using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace dnt.api.Extensions
{
    using dnt.core.Models.Response;

    public static class ErrorResponseExtensions
    {
        public static IActionResult ToActionResult(this ErrorResponse response, HttpStatusCode statusCode)
        {
            return new JsonResult(response)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}

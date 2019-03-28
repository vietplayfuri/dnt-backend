using System.Threading.Tasks;
using dnt.core.Models.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace dnt.api.Middleware
{
    public class OnCompleteMiddleware
    {
        private readonly RequestDelegate _next;

        public OnCompleteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
            if (context.Response.StatusCode == 404)
            {
                var errorMsg = context.Request.Path.HasValue ? $"Endpoint '{context.Request.Path.Value}' not found." : "No endpoint in request.";
                var response = JsonConvert.SerializeObject(new ErrorResponse(errorMsg));
                await context.Response.WriteAsync(response);
            }
        }
    }
}

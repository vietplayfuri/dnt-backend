namespace dnt.api.Exceptions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Serilog;

    public class GlobalExceptionHandler
    {
        public static Action<IApplicationBuilder> HandleException()
        {
            return builder =>
            {
                builder.Run(context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        Log.ForContext<GlobalExceptionHandler>().Error(error.Error.Message);
                    }
                    return Task.CompletedTask;
                });
            };
        }
    }
}

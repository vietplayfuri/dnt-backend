namespace dnt.api.Filters
{
    using dnt.core.Exceptions;
    using dnt.core.Models.Response;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Serilog;

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public GlobalExceptionFilter()
        {
            _logger = Log.ForContext<GlobalExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            var response = new ErrorResponse(context.Exception.Message)
            {
                StackTrace = context.Exception.StackTrace
            };

            // Support descriptive exceptions
            var exceptionWithServiceError = context.Exception as HttpException;
            if (exceptionWithServiceError != null)
            {
                context.Result = new ObjectResult(new ErrorResponse(exceptionWithServiceError.Message)
                {
                    StackTrace = exceptionWithServiceError.StackTrace
                })
                {
                    StatusCode = exceptionWithServiceError.StatusCode,
                    DeclaredType = typeof(HttpException)
                };
            }
            else
            {
                context.Result = new ObjectResult(response)
                {
                    StatusCode = 500,
                    DeclaredType = typeof(ErrorResponse)
                };
            }
            _logger.Error(context.Exception.ToString());
        }
    }
}

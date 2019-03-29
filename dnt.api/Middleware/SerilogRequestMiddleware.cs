namespace dnt.api.Middleware
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Serilog;
    using Serilog.Events;

    public class SerilogRequestMiddleware
    {
        private readonly string _messageTemplate =
            "Method: {RequestMethod} - URL: {RequestPath} - Headers: {Headers} - StatusCode: {StatusCode} - Duration: {Elapsed} ms";

        private static readonly ILogger Log = Serilog.Log.ForContext<SerilogRequestMiddleware>();

        private readonly RequestDelegate _next;

        public SerilogRequestMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var sw = Stopwatch.StartNew();
            try
            {
                if (httpContext.Response.StatusCode != (int) HttpStatusCode.Unauthorized)
                {
                    await _next(httpContext);
                }
                sw.Stop();
                var req = httpContext.Request;

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                log.Write(level, _messageTemplate, req.Method, req.Path + req.QueryString, string.Join("|", req.Headers.ToList()), statusCode,
                    (long) Math.Round(sw.Elapsed.TotalMilliseconds));
            }
            catch (Exception ex)
            {
                LogException(httpContext, sw, ex);
                throw;
            }
        }


        private void LogException(HttpContext httpContext, Stopwatch sw, Exception ex)
        {
            sw.Stop();

            LogForErrorContext(httpContext)
                .Error(ex, _messageTemplate, httpContext.Request.Method, httpContext.Request.Path + httpContext.Request.QueryString, string.Join("|", httpContext.Request.Headers.ToList()), 500, sw.Elapsed.TotalMilliseconds);
        }

        private static ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var result = Log
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
            {
                result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
            }

            return result;
        }
    }
}
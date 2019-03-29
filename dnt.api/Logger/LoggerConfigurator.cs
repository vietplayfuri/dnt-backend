namespace dnt.api.Logger
{
    using System;
    using Filters;
    using Microsoft.Extensions.Configuration;
    using Middleware;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Filters;

    public static class LoggerConfigurator
    {
        private const long FileSizeLimitBytes = 52428800;
        private const int RetainedFileCountLimit = 31;
        private const RollingInterval DailyRollingInterval = RollingInterval.Day;
        public static Logger Configure(IConfiguration configuration)
        {
            var logLevelSwtich = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };

            // Please do not change this format as this format is used by fluentd
            var template =
                "{Timestamp:yyyy-MM-ddTHH\\:mm\\:ss.ffzzz} [" + configuration.GetValue<string>("AppSettings:HostName") + "] [{Level}] [{SourceContext}] {Message} {Exception}" + Environment.NewLine;

            var loggerConfiguration =
                new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(logLevelSwtich)
                    .WriteTo.Async(b => b.Logger(l =>
                            l.Enrich.FromLogContext())
                        .WriteTo.Async(a => a.FileStaticName("logs/costs.net.incoming.http.log", outputTemplate: template, rollOnFileSizeLimit: true, fileSizeLimitBytes: FileSizeLimitBytes, rollingInterval: DailyRollingInterval, retainedFileCountLimit: RetainedFileCountLimit)
                            .Filter.ByIncludingOnly(Matching.FromSource<SerilogRequestMiddleware>()))
                        .WriteTo.Async(a =>
                            a.FileStaticName("logs/costs.net.messaging.log", outputTemplate: template, rollOnFileSizeLimit: true, fileSizeLimitBytes: FileSizeLimitBytes, rollingInterval: DailyRollingInterval, retainedFileCountLimit: RetainedFileCountLimit)
                                .Filter.ByIncludingOnly(c =>
                                    Matching.FromSource("costs.net.messaging")(c) ||
                                    Matching.FromSource("costs.net.core.Messaging")(c)))
                        .WriteTo.Async(a =>
                            a.FileStaticName("logs/costs.net.application.log", outputTemplate: template, rollOnFileSizeLimit: true, fileSizeLimitBytes: FileSizeLimitBytes, rollingInterval: DailyRollingInterval, retainedFileCountLimit: RetainedFileCountLimit)
                                .Filter.ByIncludingOnly(c =>
                                    Matching.FromSource("costs.net")(c) ||
                                    Matching.FromSource<AuthMiddleware>()(c) ||
                                    Matching.FromSource<Startup>()(c) ||
                                    Matching.FromSource<GlobalExceptionFilter>()(c))
                                .Filter.ByExcluding(c =>                                    
                                    Matching.FromSource("costs.net.core.messaging")(c) ||
                                    Matching.FromSource("costs.net.messaging")(c) ||
                                    Matching.FromSource<SerilogRequestMiddleware>()(c)))
                    .ReadFrom.Configuration(configuration));

            return loggerConfiguration.CreateLogger();
        }
    }
}

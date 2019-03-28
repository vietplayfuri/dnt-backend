using System;
using System.Collections.Generic;
using System.IO;
using Ads.Net.Acl;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacSerilogIntegration;
using costs.net.core.Models.Utils;
using costs.net.core.Modules;
using costs.net.dataAccess;
using costs.net.messaging;
using costs.net.scheduler.core;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;

namespace costs.net.scheduler.host
{
    using Microsoft.Extensions.Logging;
    using ILogger = Serilog.ILogger;

    class Startup
    {
        private static readonly ILogger Logger = Log.ForContext<Startup>();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServer, ConsoleAppRunner>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };
            // Please do not change this format as this format is used by fluentd
            var template =
                "{Timestamp:yyyy-MM-ddTHH\\:mm\\:ss.ffzzz} [" + config.GetValue<string>("AppSettings:HostName") + "] [{Level}] [{SourceContext}]  {Message} {Exception}" + Environment.NewLine;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .WriteTo.Async(a =>
                    a.File("logs/costs.net.scheduler.log", outputTemplate: template)
                        .Filter.ByIncludingOnly(c =>
                            Matching.FromSource("costs.net")(c)))
                .ReadFrom.Configuration(config)
                .CreateLogger();

            Logger.Information("Starting costs.net.scheduler.host...");
        
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                },
                TypeNameHandling = TypeNameHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddMemoryCache();
            services.AddLogging();
            services.Configure<AppSettings>(options => config.GetSection("AppSettings").Bind(options));
            services.Configure<AclSettings>(options => config.GetSection("AclSettings").Bind(options));
            services.Configure<AmqSettings>(options => config.GetSection("AmqSettings").Bind(options));
            services.Configure<ElasticSearchSettings>(options => config.GetSection("ElasticSearch").Bind(options));
            services.Configure<PaperpusherSettings>(options => config.GetSection(nameof(PaperpusherSettings)).Bind(options));
            services.Configure<CacheSettings>(options => config.GetSection("CacheSettings").Bind(options));


            var builder = new ContainerBuilder();
            builder.RegisterLogger();
            builder.RegisterInstance<IConfiguration>(config);
            builder.RegisterModule<AclModule>();
            builder.RegisterModule<SchedulerModule>();
            builder.RegisterModule<plugins.Modules.PluginModule>();
            builder.RegisterModule<MessagingModule>();
            builder.RegisterModule<ServiceModule>();
            RegisterDataAccess(config, builder);

            loggerFactory.AddSerilog();

            builder.Populate(services);
            var applicationContainer = builder.Build();

            PrintSettings(applicationContainer);
            //The FluentScheduler
            SetupJobManager(applicationContainer);
        }

        private void RegisterDataAccess(IConfigurationRoot config, ContainerBuilder builder)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<EFContext>();
            dbContextBuilder.UseNpgsql(config.GetSection("Data:DatabaseConnection:ConnectionString").Value);
            var costAdminId = Guid.Parse(config.GetSection($"{nameof(AppSettings)}:{nameof(AppSettings.CostsAdminUserId)}").Value);
            var efContextOptions = new EFContextOptions
            {
                DbContextOptions = dbContextBuilder.Options,
                SystemUserId = costAdminId
            };
            builder.RegisterInstance(efContextOptions);
            builder.RegisterModule<DataJobAccessModule>();
        }

        private void PrintSettings(IContainer applicationContainer)
        {
            var appsettings = applicationContainer.Resolve<IOptions<AppSettings>>();
            var paperPusherSettings = applicationContainer.Resolve<IOptions<PaperpusherSettings>>();
            Logger.Information(JsonConvert.SerializeObject(appsettings.Value, Formatting.Indented));
            Logger.Information(JsonConvert.SerializeObject(paperPusherSettings.Value, Formatting.Indented));
        }

        private static void SetupJobManager(IContainer container)
        {
            try
            {                
                JobManager.JobFactory = new AutofacJobFactory(container);
                JobManager.JobStart += JobManager_JobStart;
                JobManager.JobEnd += JobManager_JobEnd;
                JobManager.JobException += JobManager_JobException;
                JobManager.UseUtcTime();                
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "FluentScheduler.JobManager initialise error!");
            }
            finally
            {
                // and last shut down the scheduler when you are ready to close your program
                Logger.Information("Exiting costs.net.scheduler.host...");
                JobManager.Stop();
            }
        }

        private static void JobManager_JobStart(JobStartInfo obj)
        {
            Logger.Information($"Started job '{obj.Name}'.");
        }

        private static void JobManager_JobEnd(JobEndInfo obj)
        {
            Logger.Information($"Finished job '{obj.Name}'.");
        }

        private static void JobManager_JobException(JobExceptionInfo obj)
        {
            Logger.Error(obj.Exception, $"Error executing job '{obj.Name}'.");
        }        
    }
}

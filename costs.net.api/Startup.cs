namespace dnt.api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using AutofacSerilogIntegration;
    using dataAccess;
    using Exceptions;
    using Extensions;
    using Filters;
    using FluentValidation.AspNetCore;
    using Logger;
    using Mapping;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Microsoft.Extensions.Options;
    using Middleware;
    using MoreLinq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Security.Authentication;
    using Security.Authorization;
    using Swashbuckle.Swagger.Model;
    using dnt.core.Models.Utils;
    using Microsoft.IdentityModel.Tokens;
    using dnt.api.Security.Token;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using System.Text;
    using Microsoft.AspNetCore.Authorization;
    using dnt.core.Modules;
    using Microsoft.AspNetCore.Mvc;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            SetDefaultApplicationConfiguration();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables().Build();

            Log.Logger = LoggerConfigurator.Configure(Configuration);
        }

        public IConfigurationRoot Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddRouting(opt => opt.LowercaseUrls = true);
            //services.AddCors();
            services.AddCors(o => o.AddPolicy("MyPolicy", build =>
            {
                build.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
            }));
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll", new Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicy {  }
            //        {
            //            builder
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader();
            //        });
            //});

            // Add framework services.

            services.AddMvc()
                .AddFluentValidation(cfg =>
                {
                    cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCaseExceptDictionaryResolver();
                    options.SerializerSettings.Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    };
                })
                .AddMvcOptions(o =>
                {
                    o.Filters.Add(typeof(GlobalExceptionFilter));
                    o.Filters.Add(typeof(HttpModelFilter));
                });

            //services.AddSwaggerGen(c => c.DocumentFilter<LowercaseDocumentFilter>());
            services.AddMemoryCache();
            //services.AddCustomAuthentication();
            //services.AddCustomAuthorization();
            ConfigureToken(services);
            //services.ConfigureSwaggerGen(options =>
            //{
            //    options.SingleApiVersion(new Info
            //    {
            //        Version = "v1",
            //        Title = "Ad-Costs API",
            //        Description = "Clean Documentation of our API",
            //        TermsOfService = "None",
            //        Contact = new Contact { Name = "Copyright © 2016 Adstream Pty Ltd", Email = "hello@adstream.com", Url = "http://www.adstream.com/" },
            //        License = new License { Name = "Privacy Policy", Url = "http://www.adstream.com/privacy-policy/" }
            //    });
            //});
            //services.Configure<AppSettings>(options => Configuration.GetSection("AppSettings").Bind(options));
            //services.Configure<AmqSettings>(options => Configuration.GetSection("AmqSettings").Bind(options));
            //services.Configure<ElasticSearchSettings>(options => Configuration.GetSection("ElasticSearch").Bind(options));
            //services.Configure<AdIdSettings>(options => Configuration.GetSection(nameof(AdIdSettings)).Bind(options));
            //services.Configure<PaperpusherSettings>(options => Configuration.GetSection(nameof(PaperpusherSettings)).Bind(options));
            services.Configure<CacheSettings>(options => Configuration.GetSection("CacheSettings").Bind(options));

            var builder = new ContainerBuilder();
            builder.RegisterLogger();
            builder.RegisterInstance<IConfiguration>(Configuration);
            builder.RegisterModule<ServiceModule>();
            //builder.RegisterModule<PluginModule>();
            //builder.RegisterModule<MessagingModule>();
            builder.RegisterModule<MappingModule>();
            RegisterDataAccess(builder);

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            // PrintSettings();
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationContainer);
        }

        private void RegisterDataAccess(ContainerBuilder builder)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<EFContext>();
            dbContextBuilder.UseNpgsql(Configuration.GetSection("Data:DatabaseConnection:ConnectionString").Value);
            var costAdminId = Guid.Parse(Configuration.GetSection($"{nameof(AppSettings)}:{nameof(AppSettings.CostsAdminUserId)}").Value);
            var efContextOptions = new EFContextOptions
            {
                DbContextOptions = dbContextBuilder.Options,
                SystemUserId = 100, //costAdminId,
                IsLoggingEnabled = true
            };
            builder.RegisterInstance(efContextOptions);
            builder.RegisterModule<DataAccessModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(OnApplicationStopped);

            // Global exception handler
            app.UseExceptionHandler(GlobalExceptionHandler.HandleException());
            app.UseCors("MyPolicy");
            app.UseMvc();
            // auth middleware
            //app.UseMiddleware<AuthMiddleware>();

            // log Request middleware
            //app.UseMiddleware<SerilogRequestMiddleware>();
            //All Other middleware should go below these two since order matters in the request pipeline
            //

            // on complete middleware
            //app.UseMiddleware<OnCompleteMiddleware>();

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();

            //InitialiseAmqConnection();
        }

        private void OnApplicationStopped()
        {
            Log.CloseAndFlush();
            ApplicationContainer.Dispose();
        }

        private void PrintSettings()
        {
            var appsettings = ApplicationContainer.Resolve<IOptions<AppSettings>>();
            var amqSettings = ApplicationContainer.Resolve<IOptions<AmqSettings>>();
            var elasticSearchSettings = ApplicationContainer.Resolve<IOptions<ElasticSearchSettings>>();
            var paperPusherSettings = ApplicationContainer.Resolve<IOptions<PaperpusherSettings>>();
            var adIdSettings = ApplicationContainer.Resolve<IOptions<AdIdSettings>>();
            Log.Logger.ForContext<Startup>().Information(JsonConvert.SerializeObject(appsettings.Value, Formatting.None));
            Log.Logger.ForContext<Startup>().Information(JsonConvert.SerializeObject(amqSettings.Value, Formatting.None));
            Log.Logger.ForContext<Startup>().Information(JsonConvert.SerializeObject(paperPusherSettings.Value, Formatting.None));
            Log.Logger.ForContext<Startup>().Information(JsonConvert.SerializeObject(adIdSettings.Value, Formatting.None));
            Log.Logger.ForContext<Startup>().Information(JsonConvert.SerializeObject(elasticSearchSettings.Value, Formatting.None));
        }

        private void InitialiseAmqConnection()
        {
            //ApplicationContainer.Resolve<IEnumerable<IMessageReceiver>>().ForEach(b => b.ActivateAsync());
            //ApplicationContainer.Resolve<IEnumerable<IMessageSender>>().ForEach(b => b.ActivateAsync());
        }

        private static void SetDefaultApplicationConfiguration()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                },
                TypeNameHandling = TypeNameHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        }


        public void ConfigureToken(IServiceCollection services)
        {
            string secretKey = "mysite_supersecret_secretkey!8050";

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        }
    }
}

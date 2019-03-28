namespace costs.net.integration.tests
{
    using System.Collections.Generic;
    using api;
    using api.Extensions;
    using Browser;
    using core.ExternalResource.Gdam;
    using core.ExternalResource.Paperpusher;
    using core.Messaging;
    using core.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using net.tests.common.Stubs;
    using Nest;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public sealed class ApiTestContext
    {
        private ApiTestContext()
        {}

        public static ApiTestContext Instance { get; } = Init();

        public BrowserShim Browser { get; private set; }
        public TestServer TestServer { get; private set; }
        public Mock<IElasticClient> ElasticClient { get; private set; }

        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCaseExceptDictionaryResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        private static ApiTestContext Init()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            var elasticSearchServiceMock = new Mock<IElasticClient>();
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddScoped(typeof(IPermissionService), typeof(PermissionServiceStub));
                    services.AddSingleton<IGdamClient>(sp => new GdamClientStub());
                    services.AddSingleton(new Mock<IInternalMessageSender>().Object);
                    services.AddSingleton(new Mock<IExternalMessageReceiver>().Object);
                    services.AddSingleton(typeof(IPaperpusherClient), typeof(PaperpusherClientStub));
                    services.AddSingleton(typeof(IElasticClient), elasticSearchServiceMock.Object);
                })
                .UseConfiguration(configuration);

            var testServer = new TestServer(webHostBuilder);
            var client = testServer.CreateClient();
            var instance = new ApiTestContext
            {
                TestServer = testServer,
                Browser = new BrowserShim(client),
                ElasticClient = elasticSearchServiceMock
            };
            return instance;
        }
    }
}
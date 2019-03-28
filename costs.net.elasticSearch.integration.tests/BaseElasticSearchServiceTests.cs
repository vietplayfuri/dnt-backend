namespace costs.net.elasticSearch.integration.tests
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders;
    using core.Builders.Response;
    using core.Builders.Search;
    using core.Models;
    using core.Models.Utils;
    using core.Services.Search;
    using dataAccess;
    using Elasticsearch.Net;
    using ElasticsearchInside;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Nest;
    using NUnit.Framework;
    using Serilog;

    [TestFixture]
    public class BaseElasticSearchServiceTests
    {
        protected Elasticsearch Elasticsearch;
        protected IElasticClient ElasticClient;
        protected Mock<IOptions<AppSettings>> AppSettingsOptionsMock;
        protected Mock<IMapper> MapperMock;
        protected Mock<ILogger> LoggerMock;
        protected EFContext EFContext;
        protected Mock<ICostSearchBuilder> CostSearchBuilderMock;
        protected IElasticSearchIndexService ElasticSearchIndexService;

        /// <summary>
        ///     Initialises ElasticSearch, ElasticSearchClient and indexes. Initialise once per each test class to improve performance of integration tests
        /// </summary>
        /// <returns></returns>
        [OneTimeSetUp]
        protected async Task SetupOnce()
        {
            Elasticsearch = await new Elasticsearch().Ready();
            ElasticClient = new ElasticClient(new ConnectionSettings(Elasticsearch.Url));
            AppSettingsOptionsMock = new Mock<IOptions<AppSettings>>();
            AppSettingsOptionsMock.Setup(o => o.Value).Returns(new AppSettings
            {
                AdminUser = "4ef31ce1766ec96769b399c0"
            });
            EFContext = EFContextFactory.CreateInMemoryEFContext();

            MapperMock = new Mock<IMapper>();
            LoggerMock = new Mock<ILogger>();
            CostSearchBuilderMock = new Mock<ICostSearchBuilder>();

            ElasticSearchIndexService = new ElasticSearchIndexService(
                new[]
                {
                    new Lazy<ICostSearchBuilder, PluginMetadata>(() => CostSearchBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
                },
                LoggerMock.Object,
                EFContext,
                MapperMock.Object,
                ElasticClient,
                AppSettingsOptionsMock.Object
                );

            await OneTimeSetup();

            await CreateIndexes();
        }

        protected virtual async Task CreateIndexes()
        {
            await ElasticSearchIndexService.CreateIndices();
        }

        protected Task<IIndexResponse> AddToIndex<T>(T searchItem, string index) where T : class, ISearchItem
        {
            return ElasticClient.IndexAsync(searchItem,
                u => u.Id(searchItem.Id)
                    .VersionType(VersionType.ExternalGte)
                    .Version(searchItem.Version)
                    .Index(index)
                    .Refresh(Refresh.WaitFor));
        }

        protected virtual Task OneTimeSetup()
        {
            return Task.CompletedTask;
        }

    }
}

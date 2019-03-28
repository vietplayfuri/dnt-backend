namespace costs.net.elasticSearch.integration.tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders;
    using core.Builders.Search;
    using core.Models;
    using core.Models.Utils;
    using core.Services.Search;
    using dataAccess;
    using ElasticsearchInside;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Nest;
    using NUnit.Framework;
    using Serilog;
    using costs.net.core;
    using System.Linq;

    [TestFixture]
    public class ElasticSearchIndexTests
    {
        private Elasticsearch _elasticsearch;
        private IElasticSearchIndexService _elasticSearchIndexService;
        private IEnumerable<Lazy<ICostSearchBuilder, PluginMetadata>> _pluginMetadataServices;
        private Mock<ICostSearchBuilder> _costSearchBuilderMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger> _loggerMock;
        private IElasticClient _elasticClient;
        private EFContext _efContext;

        [SetUp]
        public async Task Init()
        {
            _costSearchBuilderMock = new Mock<ICostSearchBuilder>();
            _loggerMock = new Mock<ILogger>();
            _mapperMock = new Mock<IMapper>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();

            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _pluginMetadataServices = new[]
            {
                new Lazy<ICostSearchBuilder, PluginMetadata>(() => _costSearchBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
            };
            _elasticsearch = new Elasticsearch();
            await _elasticsearch.Ready();
            _elasticClient = new ElasticClient(new ConnectionSettings(_elasticsearch.Url));

            _elasticSearchIndexService = new ElasticSearchIndexService(
                _pluginMetadataServices, _loggerMock.Object, _efContext, _mapperMock.Object, _elasticClient,_appSettingsMock.Object);

        }
        [Test]
        public void Ping_ElasticSearch()
        {
            // Arrange

            // Act
            var result = _elasticClient.Ping();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Create_Indices()
        {
            // Arrange
            // Act
            await _elasticSearchIndexService.CreateIndices();

            // Assert
            var indecisResult = await _elasticClient.CatIndicesAsync();

            indecisResult.Records.Count.Should().BeGreaterThan(1);
            indecisResult.Records.Count.Should().Be(5);
        }

        [Test]
        public async Task Create_Indices_when_index_is_incorrectly_named_same_as_index_alias()
        {
            // Arrange
            await _elasticClient.CreateIndexAsync(Constants.ElasticSearchIndices.ProjectsIndexName);

            // Act
            var indecisResult = await _elasticClient.CatIndicesAsync();

            await _elasticSearchIndexService.CreateIndices();

            var createdIndecisResult = await _elasticClient.CatIndicesAsync();

            // Assert
            indecisResult.Records.Count.Should().Be(1);
            indecisResult.Records.Should().Contain((index) => index.Index == Constants.ElasticSearchIndices.ProjectsIndexName);

            createdIndecisResult.Records.Count.Should().BeGreaterThan(1);
            createdIndecisResult.Records.Count.Should().Be(5);
            createdIndecisResult.Records.Should().NotContain((index) => index.Index == Constants.ElasticSearchIndices.ProjectsIndexName);
        }
    }
}

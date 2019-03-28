namespace costs.net.core.tests.Services.ElasticSearch
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using Builders;
    using Builders.Search;
    using core.Services;
    using core.Services.Search;
    using dataAccess;
    using Serilog;
    using Microsoft.Extensions.Options;
    using core.Models;
    using core.Models.Utils;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Nest;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticSearchServiceTest
    {
        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _elasticSettingsMock = new Mock<IOptions<ElasticSearchSettings>>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings
            {
                ElasticBatchSize = 1000
            });
            _elasticClientMock = new Mock<IElasticClient>();
            _elasticSettingsMock.Setup(a => a.Value).Returns(new ElasticSearchSettings
            {
                Nodes = "http://localhost:9222",
                IsLogged = true,
                DefaultIndex = "costs",
                DefaultSearchSize = 50
            });
            _permissionServiceMock = new Mock<IPermissionService>();
            _mapperMock = new Mock<IMapper>();
            _logger = new Mock<ILogger>();
            _costSearchBuilderMock = new Mock<ICostSearchBuilder>();
            _costViewDetailsMock = new Mock<ICostViewDetails>();

            _elasticSearchService = new ElasticSearchService(
                new List<Lazy<ICostSearchBuilder, PluginMetadata>>
                {
                new Lazy<ICostSearchBuilder, PluginMetadata>(() => _costSearchBuilderMock.Object,
                    new PluginMetadata { BuType =  BuType.Pg })
                },
                _logger.Object,
                _permissionServiceMock.Object,
                _efContext,
                _mapperMock.Object, _elasticClientMock.Object,
                new List<Lazy<ICostViewDetails, PluginMetadata>>
                {
                new Lazy<ICostViewDetails, PluginMetadata>(() => _costViewDetailsMock.Object,
                    new PluginMetadata { BuType =  BuType.Pg })
                }
                );
        }

        private ElasticSearchService _elasticSearchService;
        private Mock<IOptions<ElasticSearchSettings>> _elasticSettingsMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IElasticClient> _elasticClientMock;
        private Mock<ICostViewDetails> _costViewDetailsMock;
        private Mock<IMapper> _mapperMock;
        private EFContext _efContext;
        private Mock<ILogger> _logger;
        private Mock<ICostSearchBuilder> _costSearchBuilderMock;
        private Mock<IPermissionService> _permissionServiceMock;
    }
}

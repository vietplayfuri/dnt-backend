
namespace costs.net.core.tests.Services.Admin
{
    using System.Threading.Tasks;
    using core.Services.Admin;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Ads.Net.Acl;
    using core.Models.Utils;
    using dataAccess;
    using Microsoft.Extensions.Options;

    [TestFixture]
    public class StatusServiceTests
    {
        private readonly Mock<IOptions<AclSettings>> _aclSettingsMock = new Mock<IOptions<AclSettings>>();
        private readonly Mock<IOptions<AppSettings>> _appSettingsMock = new Mock<IOptions<AppSettings>>();
        private readonly Mock<IOptions<ElasticSearchSettings>> _esSettingsMock = new Mock<IOptions<ElasticSearchSettings>>();
        private readonly Mock<IOptions<AmqSettings>> _amqSettingsMock = new Mock<IOptions<AmqSettings>>();
        private readonly Mock<IOptions<PaperpusherSettings>> _paperPusherSettingsMock = new Mock<IOptions<PaperpusherSettings>>();
        private readonly Mock<EFContext> _eFContextMock = new Mock<EFContext>();
        private StatusService _statusService;

        [SetUp]
        public void Init()
        {
            _aclSettingsMock.Setup(a => a.Value).Returns(new AclSettings
            {
                AclDb = "http://localhost:8529"
            });
            _amqSettingsMock.Setup(a => a.Value).Returns(new AmqSettings
            {
                AmqHost = "http://internal.dev:6519",
                AmqHostExternal = "http://external.dev:6519"
            });
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings
            {
                GdamCoreHost = "http://localhost:8080/"
            }); _esSettingsMock.Setup(a => a.Value).Returns(new ElasticSearchSettings
            {
                Nodes = "http://localhost:9200"
            });
            
            _statusService = new StatusService(_eFContextMock.Object,
                _aclSettingsMock.Object,
                _esSettingsMock.Object,
                _amqSettingsMock.Object,
                _paperPusherSettingsMock.Object,
                _appSettingsMock.Object
            );
        }

        [Test]
        public async Task Get_Status()
        {
            //Arrange
            // cannot simulate or replace the HttpClient class inside StatusService and so 
            // any test will always return red.
            const string expected = "red";

            //Act
            var result = await _statusService.GetStatus();

            //Assert
            result.Should().NotBeNull();
            result.Status.Should().NotBeNull();
            result.Status.Should().Be(expected);
        }
    }
}

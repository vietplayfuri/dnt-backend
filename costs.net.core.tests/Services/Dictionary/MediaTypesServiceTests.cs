namespace costs.net.core.tests.Services.Dictionary
{
    using System;
    using System.Threading.Tasks;
    using Builders;
    using core.Models;
    using core.Models.User;
    using core.Services.Dictionary;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MediaTypesServiceTests
    {
        [SetUp]
        public void Init()
        {
            _pluginServiceMock = new Mock<IMediaTypesPluginService>();

            _mediaTypesService = new MediaTypesService(new[]
            {
                new Lazy<IMediaTypesPluginService, PluginMetadata>(
                    () => _pluginServiceMock.Object,
                    new PluginMetadata { BuType = BuType.Pg }
                )
            });
        }

        private Mock<IMediaTypesPluginService> _pluginServiceMock;
        private MediaTypesService _mediaTypesService;

        [Test]
        public async Task GetMediaTypesByCostStageRevision_Always_ShouldGetMediaTypesFromPlugin()
        {
            // Arrange
            var user = new UserIdentity { BuType = BuType.Pg };
            var revisionId = Guid.NewGuid();
            var dictionaries = new[] { new DictionaryEntry() };
            _pluginServiceMock.Setup(p => p.GetMediaTypes(revisionId)).ReturnsAsync(dictionaries);

            // Act
            var result = await _mediaTypesService.GetMediaTypesByCostStageRevision(user, revisionId);

            // Assert
            _pluginServiceMock.Verify(p => p.GetMediaTypes(revisionId), Times.Once);
            result.Should().BeSameAs(dictionaries);
        }
    }
}
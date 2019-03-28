
namespace costs.net.core.tests.Services.Admin
{
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Services.Admin;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Mapping;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using core.Mapping;

    [TestFixture]
    public class FeatureServiceTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();

        private FeatureService _featureService;

        [SetUp]
        public void Init()
        {
            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(AdminProfile)
                )
            );
            IMapper mapper = new Mapper(configuration);
            _featureService = new FeatureService(_efContextMock.Object, mapper);
        }

        [Test]
        public async Task Get_EnabledFeatures()
        {
            //Arrange
            const string enabledFeatureName = "TestFeature";
            const string disabledFeatureName = "TestFeature2";
            var enabledFeature = new Feature
            {
                Name = enabledFeatureName,
                Enabled = true
            };
            var disabledFeature = new Feature
            {
                Name = disabledFeatureName,
                Enabled = false
            };
            var features = new List<Feature> { enabledFeature, disabledFeature };
            _efContextMock.MockAsyncQueryable(features.AsQueryable(), c => c.Feature);

            //Act
            var result = await _featureService.Get();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Should().NotBeNull();
            result.First().Name.Should().NotBeNull();
            result.First().Name.Should().Be(enabledFeatureName);
            result.First().Enabled.Should().BeTrue();
        }

        [Test]
        public async Task Get_DisabledFeatures()
        {
            const string enabledFeatureName = "TestFeature";
            const string disabledFeatureName = "TestFeature2";
            var enabledFeature = new Feature
            {
                Name = enabledFeatureName,
                Enabled = true
            };
            var disabledFeature = new Feature
            {
                Name = disabledFeatureName,
                Enabled = false
            };
            var features = new List<Feature> { enabledFeature, disabledFeature };
            _efContextMock.MockAsyncQueryable(features.AsQueryable(), c => c.Feature);

            //Act
            var result = await _featureService.Get(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Should().NotBeNull();
            result.First().Name.Should().NotBeNull();
            result.First().Name.Should().Be(disabledFeatureName);
            result.First().Enabled.Should().BeFalse();
        }
    }
}

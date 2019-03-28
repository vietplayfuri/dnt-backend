
namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models.PerDiem;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class PerDiemProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<PerDiemProfile>();
            }));
        }

        [Test]
        public void PerDiem_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Entity_To_Model_IsValid()
        {
            var expectedCountry = "UK";
            var expectedRegion = "Europe";
            var expectedIsDefault = false;
            var expectedCity = "Bradford";
            var expectedCost = 100;
            var entity = new PerDiem
            {
                Cost = expectedCost,
                Id = Guid.NewGuid(),
                Country = expectedCountry,
                Region = expectedRegion,
                IsDefault = expectedIsDefault,
                ShootCity = expectedCity
            };
            var result = _mapper.Map<PerDiem, PerDiemModel>(entity);

            result.Should().NotBeNull();
            result.Cost.Should().Be(expectedCost);
            result.Id.Should().Be(entity.Id);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(entity.Country);
            result.Region.Should().NotBeNull();
            result.Region.Should().Be(entity.Region);
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(entity.ShootCity);
            result.IsDefault.Should().Be(entity.IsDefault);
        }

        [Test]
        public void Model_To_Entity_IsValid()
        {
            var expectedCountry = "UK";
            var expectedRegion = "Europe";
            var expectedIsDefault = false;
            var expectedCity = "Bradford";
            var expectedCost = 100;
            var model = new PerDiemModel
            {
                Cost = expectedCost,
                Id = Guid.NewGuid(),
                Country = expectedCountry,
                Region = expectedRegion,
                IsDefault = expectedIsDefault,
                ShootCity = expectedCity
            };
            var result = _mapper.Map<PerDiemModel, PerDiem>(model);

            result.Should().NotBeNull();
            result.Cost.Should().Be(expectedCost);
            result.Id.Should().Be(model.Id);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(model.Country);
            result.Region.Should().NotBeNull();
            result.Region.Should().Be(model.Region);
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(model.ShootCity);
            result.IsDefault.Should().Be(model.IsDefault);
        }
    }
}

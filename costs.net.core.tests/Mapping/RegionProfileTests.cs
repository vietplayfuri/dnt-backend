namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Regions;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class RegionProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<RegionProfile>();
            }));
        }

        private Region GetRegion(int i)
        {
            return new Region
            {
                Id = Guid.NewGuid(),
                Key = $"Key {i}",
                Name = $"Name {i}"
            };
        }

        [Test]
        public void RegionModel_To_Region_IsValid()
        {
            // Arrange
            var regionModel = new RegionModel
            {
                Id = Guid.NewGuid(),
                Name = "model"
            };

            // Act
            var region = _mapper.Map<Region>(regionModel);

            // Assert
            region.Id.Should().Be(regionModel.Id.ToString());
            region.Name.Should().Be(regionModel.Name);
        }
        [Test]
        public void Region_To_RegionModel_IsValid()
        {
            // Arrange
            var region = GetRegion(1);

            // Act
            var model = _mapper.Map<RegionModel>(region);

            // Assert
            model.Id.Should().Be(region.Id.ToString());
            model.Name.Should().Be(region.Name);
        }

        [Test]
        public void GlobalAgencyRegion_To_RegionModel_IsValid()
        {
            // Arrange
            var agencyRegion = new GlobalAgencyRegion
            {
                Id = Guid.NewGuid(),
                Region = "Global Agency Region Name here"
            };

            // Act
            var model = _mapper.Map<RegionModel>(agencyRegion);

            // Assert
            model.Id.Should().Be(agencyRegion.Id.ToString());
            model.Name.Should().Be(agencyRegion.Region);
            model.Key.Should().Be(agencyRegion.Region);
        }

        [Test]
        public void Regions_To_RegionModels_IsValid()
        {
            // Arrange
            var region1 = GetRegion(1);
            var region2 = GetRegion(2);
            var region3 = GetRegion(3);
            var region4 = GetRegion(4);
            var regions = new List<Region> { region1, region2, region3, region4 };

            // Act
            var model = _mapper.Map<IEnumerable<RegionModel>>(regions);

            // Assert
            model.Count().Should().Be(4);
        }

        [Test]
        public void RegionProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
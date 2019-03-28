namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using core.Mapping;
    using core.Models.BusinessRole;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class BrandProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => { m.AddProfile<BrandProfile>(); }));
        }

        [Test]
        public void Brand_To_BrandModel_IsValid()
        {
            // Arrange
            var creatorId = Guid.NewGuid();
            var brand = new Brand
            {
                AdIdPrefix = "ADC1",
                Created = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                CreatedById = creatorId,
                Modified = DateTime.UtcNow,
                Name = "New_Brand",
                Sector = new Sector
                {
                    AgencyId = Guid.NewGuid(),
                    Brands = new List<Brand>(),
                    Created = DateTime.Now,
                    CreatedById = creatorId,
                    Id = Guid.NewGuid(),
                    Modified = DateTime.Now,
                    Name = "SEcot r21"
                }
            };
            // Act
            var model = _mapper.Map<BrandModel>(brand);

            // Assert
            model.Id.Should().Be(brand.Id);

            model.Sector.Should().NotBeNull();
            model.Sector.Name.Should().Be(brand.Sector.Name);

            model.Sector.Id.Should().Be(brand.Sector.Id);
        }

        [Test]
        public void CostModelProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}

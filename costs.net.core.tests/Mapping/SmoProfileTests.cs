namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Smo;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class SmoProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<SmoProfile>()));
        }

        private Smo GetSmo()
        {
            return new Smo
            {
                Id = Guid.NewGuid(),
                Key = "Key",
                Region = new Region(),
                RegionId = Guid.NewGuid(),
                Value = "KeyValue"
            };
        }

        [Test]
        public void Smo_To_SmoModel_IsValid()
        {
            // Arrange
            var smo = GetSmo();

            // Act
            var model = _mapper.Map<SmoModel>(smo);

            // Assert
            model.Id.Should().Be(smo.Id.ToString());
            model.Name.Should().Be(smo.Value);
        }

        [Test]
        public void SearchItemProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
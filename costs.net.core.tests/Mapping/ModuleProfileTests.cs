namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    class ModuleProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<ModuleProfile>()));
        }

        [Test]
        public void AgencyProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void AbstractType_To_AgencyModel_IsValid()
        {
            // Arrange
            var module = new Module
            {
                Id = Guid.NewGuid(),
                ClientType = ClientType.Pg,
                Key = "P&G",
                Name = "PG agency name",
                AbstractType = new AbstractType
                {
                    Id = Guid.NewGuid()
                }
            };

            // Act
            var model = _mapper.Map<Module, core.Models.AbstractTypes.Module>(module);

            // Assert
            model.Id.Should().Be(module.AbstractType.Id);
            model.Key.Should().Be(model.Key);
            model.Name.Should().Be(model.Name);
            model.BuType.Should().Be((BuType)module.ClientType);
        }
    }
}

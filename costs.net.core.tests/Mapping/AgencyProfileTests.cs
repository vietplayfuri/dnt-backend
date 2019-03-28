namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Agency;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class AgencyProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<AgencyProfile>()));
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
            var agencyCountryId = Guid.NewGuid();
            var abstractTypeAgency = new AbstractType
            {
                Id = Guid.NewGuid(),
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Name = "PG agency name",
                    Labels = new[] { Constants.BusinessUnit.CostModulePrimaryLabelPrefix + "PG" },
                    CountryId = agencyCountryId,
                    Country = new Country
                    {
                        Id = agencyCountryId,
                        Iso = "Country iso",
                        Name = "Country name",
                    }
                }
            };

            // Act
            var model = _mapper.Map<AbstractType, AgencyModel>(abstractTypeAgency);

            // Assert
            model.Id.Should().Be(abstractTypeAgency.ObjectId);
            model.AbstractTypeId.Should().Be(abstractTypeAgency.Id);
            model.Name.Should().Be(abstractTypeAgency.Agency.Name);
            model.CountryId.Should().Be(agencyCountryId);
            model.CountryIso.Should().Be(abstractTypeAgency.Agency.Country.Iso);
            model.CountryName.Should().Be(abstractTypeAgency.Agency.Country.Name);
        }
    }
}
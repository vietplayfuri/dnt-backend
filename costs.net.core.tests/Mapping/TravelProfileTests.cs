
namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Costs;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class TravelProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<TravelProfile>();
            }));
        }

        [Test]
        public void TravelCost_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Entity_To_Model_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCity = "Bradford";
            var expectedUniqueItemId = Guid.NewGuid();
            var expectedTravelTypeCost = 25M;
            var expectedTravelType = "air travel";
            var expectedTotalCost = 35M;
            var expectedShootDays = 20;
            var expectedRole = "traveller";
            var expectedPerDiem = 32.5M;
            var expectedComment = "no comment";
            var expectedCostStageRevisionId = Guid.NewGuid();
            var expectedLinkContentType = new []{"ABC", "123"};
            var expectedName = "a name";
            var expectedOtherContentType = false;
            var entity = new TravelCost
            {
                Id = expectedId,
                ShootCity = expectedCity,
                Comments = expectedComment,
                CostStageRevisionId = expectedCostStageRevisionId,
                LinkContentType = expectedLinkContentType,
                Name = expectedName,
                OtherContentType = expectedOtherContentType,
                PerDiem = expectedPerDiem,
                Role = expectedRole,
                ShootDays = expectedShootDays,
                TotalCost = expectedTotalCost,
                TravelType = expectedTravelType,
                TravelTypeCost = expectedTravelTypeCost,
                UniqueItemId = expectedUniqueItemId               
            };
            var result = _mapper.Map<TravelCost, TravelCostModel>(entity);

            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.Region.Should().BeNull();
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(entity.ShootCity);
            result.Id.Should().Be(expectedId);
            result.Comments.Should().Be(expectedComment);
            result.LinkContentType.Should().BeEquivalentTo(expectedLinkContentType);
            result.Name.Should().Be(expectedName);
            result.OtherContentType.Should().Be(expectedOtherContentType);
            result.PerDiem.Should().Be(expectedPerDiem);
            result.Region.Should().BeNull();
            result.Role.Should().Be(expectedRole);
            result.ShootDays.Should().Be(expectedShootDays);
            result.TotalCost.Should().Be(expectedTotalCost);
            result.TravelType.Should().Be(expectedTravelType);
            result.TravelTypeCost.Should().Be(expectedTravelTypeCost);
            result.UniqueItemId.Should().Be(expectedUniqueItemId);
        }

        [Test]
        public void CountryId_Is_EmptyGuid_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCity = "All Other Locations";
            var expectedCountry = "All Other Locations";
            var expectedCost = 100;
            var expectedUniqueItemId = Guid.NewGuid();
            var expectedTravelTypeCost = 25M;
            var expectedTravelType = "air travel";
            var expectedTotalCost = 35M;
            var expectedShootDays = 20;
            var expectedRole = "traveller";
            var expectedPerDiem = 32.5M;
            var expectedComment = "no comment";
            var expectedCostStageRevisionId = Guid.NewGuid();
            var expectedLinkContentType = new[] { "ABC", "123" };
            var expectedName = "a name";
            var expectedOtherContentType = false;
            var entity = new TravelCost
            {
                Id = expectedId,
                ShootCity = expectedCity,
                CountryId = Guid.Empty,
                Comments = expectedComment,
                CostStageRevisionId = expectedCostStageRevisionId,
                LinkContentType = expectedLinkContentType,
                Name = expectedName,
                OtherContentType = expectedOtherContentType,
                PerDiem = expectedPerDiem,
                Role = expectedRole,
                ShootDays = expectedShootDays,
                TotalCost = expectedTotalCost,
                TravelType = expectedTravelType,
                TravelTypeCost = expectedTravelTypeCost,
                UniqueItemId = expectedUniqueItemId
            };
            var result = _mapper.Map<TravelCost, TravelCostModel>(entity);

            result.Should().NotBeNull();
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(entity.ShootCity);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(expectedCountry);
        }

        [Test]
        public void CountryId_Is_EmptyGuid_NoShootCity_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCity = "";
            var expectedCountry = "";
            var expectedCost = 100;
            var expectedUniqueItemId = Guid.NewGuid();
            var expectedTravelTypeCost = 25M;
            var expectedTravelType = "air travel";
            var expectedTotalCost = 35M;
            var expectedShootDays = 20;
            var expectedRole = "traveller";
            var expectedPerDiem = 32.5M;
            var expectedComment = "no comment";
            var expectedCostStageRevisionId = Guid.NewGuid();
            var expectedLinkContentType = new[] { "ABC", "123" };
            var expectedName = "a name";
            var expectedOtherContentType = false;
            var entity = new TravelCost
            {
                Id = expectedId,
                ShootCity = expectedCity,
                CountryId = Guid.Empty,
                Comments = expectedComment,
                CostStageRevisionId = expectedCostStageRevisionId,
                LinkContentType = expectedLinkContentType,
                Name = expectedName,
                OtherContentType = expectedOtherContentType,
                PerDiem = expectedPerDiem,
                Role = expectedRole,
                ShootDays = expectedShootDays,
                TotalCost = expectedTotalCost,
                TravelType = expectedTravelType,
                TravelTypeCost = expectedTravelTypeCost,
                UniqueItemId = expectedUniqueItemId
            };
            var result = _mapper.Map<TravelCost, TravelCostModel>(entity);

            result.Should().NotBeNull();
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(entity.ShootCity);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(expectedCountry);
        }

        [Test]
        public void CountryId_Is_Not_EmptyGuid_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCity = "";
            var expectedCountry = "England";
            var expectedCost = 100;
            var expectedUniqueItemId = Guid.NewGuid();
            var expectedTravelTypeCost = 25M;
            var expectedTravelType = "air travel";
            var expectedTotalCost = 35M;
            var expectedShootDays = 20;
            var expectedRole = "traveller";
            var expectedPerDiem = 32.5M;
            var expectedComment = "no comment";
            var expectedCostStageRevisionId = Guid.NewGuid();
            var expectedLinkContentType = new[] { "ABC", "123" };
            var expectedName = "a name";
            var expectedOtherContentType = false;
            var entity = new TravelCost
            {
                Id = expectedId,
                ShootCity = expectedCity,
                CountryId = Guid.NewGuid(),
                Country = new Country{ Name = expectedCountry },
                Comments = expectedComment,
                CostStageRevisionId = expectedCostStageRevisionId,
                LinkContentType = expectedLinkContentType,
                Name = expectedName,
                OtherContentType = expectedOtherContentType,
                PerDiem = expectedPerDiem,
                Role = expectedRole,
                ShootDays = expectedShootDays,
                TotalCost = expectedTotalCost,
                TravelType = expectedTravelType,
                TravelTypeCost = expectedTravelTypeCost,
                UniqueItemId = expectedUniqueItemId
            };
            var result = _mapper.Map<TravelCost, TravelCostModel>(entity);

            result.Should().NotBeNull();
            result.Region.Should().BeNull();
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(entity.ShootCity);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(expectedCountry);
        }

        [Test]
        public void Model_To_Entity_IsValid()
        {/*
            var expectedCountry = "UK";
            var expectedRegion = "Europe";
            var expectedIsDefault = false;
            var expectedCity = "Bradford";
            var expectedCost = 100;
            var model = new TravelCostModel
            {
                Cost = expectedCost,
                Id = Guid.NewGuid(),
                Country = expectedCountry,
                Region = expectedRegion,
                IsDefault = expectedIsDefault,
                ShootCity = expectedCity
            };
            var result = _mapper.Map<TravelCostModel, TravelCost>(model);

            result.Should().NotBeNull();
            result.Cost.Should().Be(expectedCost);
            result.Id.Should().Be(model.Id);
            result.Country.Should().NotBeNull();
            result.Country.Should().Be(model.Country);
            result.Region.Should().NotBeNull();
            result.Region.Should().Be(model.Region);
            result.ShootCity.Should().NotBeNull();
            result.ShootCity.Should().Be(model.ShootCity);
            result.IsDefault.Should().Be(model.IsDefault);*/
        }
    }
}

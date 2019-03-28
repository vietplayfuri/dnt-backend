
namespace costs.net.core.tests.Services.PerDiems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Mapping;
    using core.Services.PerDiems;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class PerDiemServiceTests
    {
        private PerDiemService _target;
        private Mock<EFContext> _efContextMock;
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();

            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(PerDiemProfile)
                )
            );
            _mapper = new Mapper(configuration);
            _target = new PerDiemService(_efContextMock.Object,
                _mapper);
        }

        [Test]
        public async Task Get_All()
        {
            //Arrange
            var expectedCount = 2;
            var perDiems = new List<PerDiem>
            {
                new PerDiem
                {
                    Id = Guid.NewGuid()
                },
                new PerDiem
                {
                    Id = Guid.NewGuid()
                },
            };
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetAsync();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_All_Regions()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var expectedCount = 2;
            var perDiems = new List<PerDiem>
            {
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region2
                },
            };
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetRegions();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Should().Be(region1);
            result.ToArray()[1].Should().Be(region2);
        }

        [Test]
        public async Task Get_All_Regions_No_Duplicates()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var expectedCount = 2;
            var perDiems = new List<PerDiem>
            {
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region2
                },
            };
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetRegions();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.First().Should().Be(region1);
            result.ToArray()[1].Should().Be(region2);
        }

        [Test]
        public async Task Get_All_Countries_By_Region()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var expectedCount = 2;
            var perDiems = GetPerdiemsCountryTestData(region1, region2, country1, country2, country3);
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCountries(region1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            // alphabetical sort
            result.First().Should().Be(country2);
            result.ToArray()[1].Should().Be(country1);
        }

        [Test]
        public async Task Get_All_Countries_Null_Region_ReturnsEmpty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCountryTestData(region1, region2, country1, country2, country3);
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCountries(null);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_All_Countries_EmptyString_Region_ReturnsEmpty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCountryTestData(region1, region2, country1, country2, country3);
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCountries(string.Empty);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_All_Countries_By_Region_No_Duplicates()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var expectedCount = 2;
            var perDiems = new List<PerDiem>
            {
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country2
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region2,
                    Country = country3
                }
            };
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCountries(region1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            // alphabetical sort
            result.First().Should().Be(country2);
            result.ToArray()[1].Should().Be(country1);
        }

        [Test]
        public async Task Get_All_Cities()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var city1 = "London";
            var city2 = "Bradford";
            var city3 = "Birmingham";
            var city4 = "Manchester";
            var expectedCount = 4;
            var perDiems = GetPerdiemsCityTestData(region1, region2, country1, country2, country3, city1, city2, city3, city4);
            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCities(region1, country1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            // alphabetical sort
            result.First().ShootCity.Should().Be(city3);
            result.ToArray()[1].ShootCity.Should().Be(city2);
            result.ToArray()[2].ShootCity.Should().Be(city1);
            result.ToArray()[3].ShootCity.Should().Be(city4);
        }

        [Test]
        public async Task Get_Cities_Null_Region_Returns_Empty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var city1 = "London";
            var city2 = "Bradford";
            var city3 = "Birmingham";
            var city4 = "Manchester";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCityTestData(region1, region2, country1, country2, country3, city1, city2, city3, city4);

            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCities(null, country1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_Cities_EmptyString_Region_Returns_Empty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var city1 = "London";
            var city2 = "Bradford";
            var city3 = "Birmingham";
            var city4 = "Manchester";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCityTestData(region1, region2, country1, country2, country3, city1, city2, city3, city4);

            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCities(string.Empty, country1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_Cities_Null_Country_Returns_Empty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var city1 = "London";
            var city2 = "Bradford";
            var city3 = "Birmingham";
            var city4 = "Manchester";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCityTestData(region1, region2, country1, country2, country3, city1, city2, city3, city4);

            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCities(region1, null);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Get_Cities_EmptyString_Country_Returns_Empty()
        {
            //Arrange
            var region1 = "Europe";
            var region2 = "North America";
            var country1 = "U.K.";
            var country2 = "France";
            var country3 = "Mexico";
            var city1 = "London";
            var city2 = "Bradford";
            var city3 = "Birmingham";
            var city4 = "Manchester";
            var expectedCount = 0;
            var perDiems = GetPerdiemsCityTestData(region1, region2, country1, country2, country3, city1, city2, city3, city4);

            _efContextMock.MockAsyncQueryable(perDiems.AsQueryable(), d => d.PerDiem);

            //Act
            var result = await _target.GetCities(region1, string.Empty);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        private static List<PerDiem> GetPerdiemsCityTestData(string region1, string region2, string country1, string country2, 
            string country3, string city1, string city2, string city3, string city4)
        {   return new List<PerDiem>
            {
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1,
                    ShootCity = city1
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1,
                    ShootCity = city2
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1,
                    ShootCity = city3
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country1,
                    ShootCity = city4
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region1,
                    Country = country2
                },
                new PerDiem
                {
                    Id = Guid.NewGuid(),
                    Region = region2,
                    Country = country3
                }
            };
        }

        private static List<PerDiem> GetPerdiemsCountryTestData(string region1, string region2, string country1, string country2, string country3)
        {
            return
                new List<PerDiem>
                {
                    new PerDiem
                    {
                        Id = Guid.NewGuid(),
                        Region = region1,
                        Country = country1
                    },
                    new PerDiem
                    {
                        Id = Guid.NewGuid(),
                        Region = region1,
                        Country = country2
                    },
                    new PerDiem
                    {
                        Id = Guid.NewGuid(),
                        Region = region2,
                        Country = country3
                    }
                };
        }
    }
}

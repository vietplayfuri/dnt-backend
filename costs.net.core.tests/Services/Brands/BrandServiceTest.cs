namespace costs.net.core.tests.Services.Brands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Services.Brand;
    using dataAccess;
    using ExternalResource.Gdam;
    using FluentAssertions;
    using core.Models.AMQ;
    using core.Models.Gdam;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Extensions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Serilog;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;

    [TestFixture]
    public class BrandServiceTest
    {
        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _loggerMock = new Mock<ILogger>();
            _mapperMock = new Mock<IMapper>();
            _gdamClientMock = new Mock<IGdamClient>();
            _brandService = new BrandService(_loggerMock.Object, _efContext, _gdamClientMock.Object, _mapperMock.Object);
            _jsonTestReader = new JsonTestReader();
        }

        private BrandService _brandService;
        private Mock<IGdamClient> _gdamClientMock;
        private EFContext _efContext;
        private Mock<ILogger> _loggerMock;
        private Mock<IMapper> _mapperMock;
        private JsonTestReader _jsonTestReader;

        [Test]
        public async Task GetBrandById_DoesntExist_Test()
        {
            //Setup

            // Act
            var brand = await _brandService.GetById(Guid.NewGuid());

            //Assert
            brand.Should().BeNull();
        }

        [Test]
        public async Task GetBrandById_Exists_Test()
        {
            //Setup
            var brandId = Guid.NewGuid();
            var brandList = new List<Brand>
            {
                new Brand
                {
                    Id = brandId,
                    Name = "BrandName",
                    AdIdPrefix = "PRE",
                    Sector = new Sector
                    {
                        AgencyId = Guid.NewGuid()
                    }
                }
            };
            _efContext.Brand.AddRange(brandList);
            _efContext.SaveChanges();

            // Act
            var brand = await _brandService.GetById(brandId);

            //Assert
            brand.Should().NotBeNull();
            brand.Name.Should().Be("BrandName");
            brand.AdIdPrefix.Should().Be("PRE");
            brand.Id.Should().Be(brandId);
        }

        [Test]
        public async Task SyncBusinessUnitBrands_Test()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;

            var advertiserDictionary =
                await _jsonTestReader.GetObject<A5DictionaryResponse>(
                    $"{basePath}{Path.DirectorySeparatorChar}Services{Path.DirectorySeparatorChar}Brands{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}advertiser_dictionary.json");
            var a5Agency = await _jsonTestReader.GetObject<A5Agency>(
                $"{basePath}{Path.DirectorySeparatorChar}Services{Path.DirectorySeparatorChar}Brands{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}a5_agency.json",
                true);
            var adcostAgency = await _jsonTestReader.GetObject<Agency>(
                $"{basePath}{Path.DirectorySeparatorChar}Services{Path.DirectorySeparatorChar}Brands{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}agency.json");
            var a5ProjectSchema = await _jsonTestReader.GetObject<A5ProjectSchema>(
                $"{basePath}{Path.DirectorySeparatorChar}Services{Path.DirectorySeparatorChar}Brands{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}asset_element_project_common.json");

            _efContext.Agency.Add(adcostAgency);

            _efContext.SaveChanges();
            _gdamClientMock.Setup(a => a.FindProjectSchemaByAgencyId(a5Agency._id)).ReturnsAsync(a5ProjectSchema);

            _gdamClientMock.Setup(a => a.GetDictionary(a5Agency._id, "advertiser")).ReturnsAsync(advertiserDictionary);
            // Act
            await _brandService.SyncBusinessUnitBrands(a5Agency);
            //Assert
            _efContext.Brand.ToList().Count.Should().Be(2);
            _gdamClientMock.Verify(a => a.GetDictionary(a5Agency._id, "advertiser"), Times.Once);
            _gdamClientMock.Verify(a => a.FindProjectSchemaByAgencyId(a5Agency._id), Times.Once);
        }
    }
}

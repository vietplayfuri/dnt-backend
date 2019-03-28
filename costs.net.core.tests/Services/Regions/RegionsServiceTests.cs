namespace costs.net.core.tests.Services.Regions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Models.Regions;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class RegionsServiceTests
    {
        private EFContext _efContext;
        private Mock<IMapper> _mapperMock;
        private RegionsService _regionsService;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _mapperMock = new Mock<IMapper>();

            _regionsService = new RegionsService(_efContext, _mapperMock.Object);
        }

        [Test] public async Task GetAgencyRegions_Always_Should_FilterRegionsByAgencyId()
        {
            // Arrange
            var agencyId1 = Guid.NewGuid();
            var agencyId2 = Guid.NewGuid();
            var regionName1 = "Region 1";
            var regionName2 = "Region ";

            _efContext.Agency.Add(new Agency
                {
                    Id = agencyId1,
                    GlobalAgencyRegion = new GlobalAgencyRegion
                    {
                        Id = Guid.NewGuid(),
                        Region = regionName1,
                        GlobalAgency = new GlobalAgency()
                    }
                });

            _efContext.Agency.Add(new Agency
                {
                    Id = agencyId2,
                    GlobalAgencyRegion = new GlobalAgencyRegion
                    {
                        Id = Guid.NewGuid(),
                        Region = regionName2,
                        GlobalAgency = new GlobalAgency()
                    }
                });
            _efContext.SaveChanges();
            
            // Act
            await _regionsService.GetAgencyRegions(agencyId1);

            // Assert
            _mapperMock.Verify(m => m.Map<IEnumerable<RegionModel>>(It.Is<IEnumerable<GlobalAgencyRegion>>(i => i.Count(gar => gar.Region == regionName1) == 1)), Times.Once);
        }
    }
}
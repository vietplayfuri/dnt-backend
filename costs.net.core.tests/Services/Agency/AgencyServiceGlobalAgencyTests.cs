namespace costs.net.core.tests.Services.Agency
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    public class AgencyServiceGlobalAgencyTests : BaseAgencyServiceTests
    {
        [Test]
        public async Task AddAgencyToDb_When_NoGlobalAgencyRegionLabel_ShouldNotCreateGlobalAgencyAndRegion()
        {
            // Arrange
            var a5Agency = await PrepareTestData();

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(0);
            EFContext.GlobalAgencyRegion.Should().HaveCount(0);
        }

        [Test]
        public async Task AddAgencyToDb_When_RegionIsMissingInGlobalAgencyRegionLabel_And_NewGlobalAgency_ShouldNotCreateGlobalAgencyAndRegion()
        {
            // Arrange
            var agencyName = "Saatchi";
            var regionName = "";
            var globalAgencyRegionLabel = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName}_{regionName}";
            var a5Agency = await PrepareTestData(globalAgencyRegionLabel);

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(0);
            EFContext.GlobalAgencyRegion.Should().HaveCount(0);
            LoggerAsMock.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task AddAgencyToDb_When_ValidGlobalAgencyRegionLabel_And_NewGlobalAgency_Should_CreateGlobalAgencyAndRegion()
        {
            // Arrange
            var agencyName = "Saatchi";
            var regionName = "Japan";
            var globalAgencyRegionLabel = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName}_{regionName}";
            var a5Agency = await PrepareTestData(globalAgencyRegionLabel);

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(1);
            EFContext.GlobalAgencyRegion.Should().HaveCount(1);
        }

        [Test]
        public async Task AddAgencyToDb_When_MultipleGlobalAgencyRegionLabels__Should_UseFirstLabel()
        {
            // Arrange
            var agencyName1 = "Saatchi1";
            var regionName1 = "Japan1";
            var agencyName2 = "Saatchi2";
            var regionName2 = "Japan2";
            var globalAgencyRegionLabel1 = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName1}_{regionName1}";
            var globalAgencyRegionLabel2 = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName2}_{regionName2}";
            var a5Agency = await PrepareTestData(globalAgencyRegionLabel1, globalAgencyRegionLabel2);

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(1);
            EFContext.GlobalAgencyRegion.Should().HaveCount(1);
            EFContext.GlobalAgency.First().Name.Should().Be(agencyName1);
            EFContext.GlobalAgencyRegion.First().Region.Should().Be(regionName1);
        }

        [Test]
        public async Task AddAgencyToDb_When_ValidGlobalAgencyRegionLabel_And_ExistingGlobalAgency_Should_AddGlobalAgencyRegion()
        {
            // Arrange
            var agencyName = "Saatchi";
            var regionName = "Japan";
            var globalAgencyRegionLabel = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName}_{regionName}";
            var a5Agency = await PrepareTestData(globalAgencyRegionLabel);
            var costAgency = EFContext.Agency.First();
            var existingGlobalAgencyRegion = new GlobalAgencyRegion
            {
                Region = "North America",
                GlobalAgency = new GlobalAgency
                {
                    Name = agencyName
                }
            };
            costAgency.GlobalAgencyRegion = existingGlobalAgencyRegion;

            EFContext.GlobalAgencyRegion.Add(existingGlobalAgencyRegion);
            EFContext.SaveChanges();

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(1);
            EFContext.GlobalAgencyRegion.Should().HaveCount(2);
        }

        [Test]
        public async Task AddAgencyToDb_When_ValidGlobalAgencyRegionLabel_And_ChanginGlobalAgencyForBU_Should_UpdateGlobalAgencyOfAgency()
        {
            // Arrange
            var agencyName = "Saatchi";
            var regionName = "Japan";
            var globalAgencyRegionLabel = $"{Constants.BusinessUnit.GlobalAgencyRegionLabelPrefix}{agencyName}_{regionName}";
            var a5Agency = await PrepareTestData(globalAgencyRegionLabel);

            var existingGlobalAgencyRegion = new GlobalAgencyRegion
            {
                Region = regionName,
                GlobalAgency = new GlobalAgency
                {
                    Name = "Any other global agency"
                }
            };


            var costAgency = EFContext.Agency.First();
            costAgency.GlobalAgencyRegion = existingGlobalAgencyRegion;

            EFContext.GlobalAgencyRegion.Add(existingGlobalAgencyRegion);
            EFContext.SaveChanges();

            // Act
            await AgencyService.AddAgencyToDb(a5Agency);

            // Assert
            EFContext.GlobalAgency.Should().HaveCount(2);
            EFContext.GlobalAgencyRegion.Should().HaveCount(2);
            costAgency.GlobalAgencyRegion.Region.Should().Be(regionName);
            costAgency.GlobalAgencyRegion.GlobalAgency.Name.Should().Be(agencyName);
        }
    }
}
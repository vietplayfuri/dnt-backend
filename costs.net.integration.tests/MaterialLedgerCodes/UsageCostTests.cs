namespace costs.net.integration.tests.MaterialLedgerCodes
{
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class UsageCostTests : BaseMaterialLedgerCodeTest
    {
        [TestCase("MGCUsageActor", plugins.Constants.UsageType.Actor, "S80141903", "33500003")]
        [TestCase("MGCUsageAthletes", plugins.Constants.UsageType.Athletes, "S80141903", "33500003")]
        [TestCase("MGCUsageBrandResidual", plugins.Constants.UsageType.BrandResidual, "S80141903", "33500003")]
        [TestCase("MGCUsageCelebrity", plugins.Constants.UsageType.Celebrity, "S80141903", "33500003")]
        [TestCase("MGCUsageCountryAiringRights", plugins.Constants.UsageType.CountryAiringRights, "S80141903", "33500003")]
        [TestCase("MGCUsageFilm", plugins.Constants.UsageType.Film, "S80141903", "33500003")]
        [TestCase("MGCUsageFootage", plugins.Constants.UsageType.Footage, "S80141903", "33500003")]
        [TestCase("MGCUsageIllustrator", plugins.Constants.UsageType.Ilustrator, "S80141903", "33500003")]
        [TestCase("MGCUsageModel", plugins.Constants.UsageType.Model, "S80141903", "33500003")]
        [TestCase("MGCUsageMusic", plugins.Constants.UsageType.Music, "S55111500", "33500004")]
        [TestCase("MGCUsageOrganization", plugins.Constants.UsageType.Organization, "S80141903", "33500003")]
        [TestCase("MGCUsagePhotography", plugins.Constants.UsageType.Photography, "S55111500", "33500004")]
        [TestCase("MGCUsageVoiceOver", plugins.Constants.UsageType.VoiceOver, "S55111500", "33500004")]
        public async Task CheckSingleMgcCodes(string costTitle, string usageType, string expectedMgCode, string expectedGlCode)
        {
            // Arrange
            var latestRevision = await SetupUsageCost(costTitle, usageType);

            // Act
            var b = await LedgerMaterialCodeService.GetLedgerMaterialCodes(latestRevision.Id);

            // Assert
            b.Should().NotBeNull();
            b.GlCode.Should().Be(expectedGlCode);
            b.MgCode.Should().Be(expectedMgCode);
        }

        private async Task<CostStageRevision> SetupUsageCost(string costTitle, string usageType)
        {
            var cost = await CreateUsageCost(User, costTitle, usageType);
            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);
            
            await LedgerMaterialCodeService.UpdateLedgerMaterialCodes(latestRevision.Id);
            return latestRevision;
        }
    }
}

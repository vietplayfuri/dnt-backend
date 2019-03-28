namespace costs.net.integration.tests.MaterialLedgerCodes
{
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DistributionCostTests : BaseMaterialLedgerCodeTest
    {
        [TestCase("MGCDistribution", "S821018AT", "33500002")]
        public async Task CheckSingleMgcCodes(string costTitle, string expectedMgCode, string expectedGlCode)
        {
            // Arrange
            var latestRevision = await SetupDistributionCost(costTitle);

            // Act
            var b = await LedgerMaterialCodeService.GetLedgerMaterialCodes(latestRevision.Id);

            // Assert
            b.Should().NotBeNull();
            b.GlCode.Should().Be(expectedGlCode);
            b.MgCode.Should().Be(expectedMgCode);
        }

        private async Task<CostStageRevision> SetupDistributionCost(string costTitle)
        {
            var cost = await CreateDistributionCostEntity(User, costTitle);
            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            await LedgerMaterialCodeService.UpdateLedgerMaterialCodes(latestRevision.Id);
            return latestRevision;
        }
    }
}

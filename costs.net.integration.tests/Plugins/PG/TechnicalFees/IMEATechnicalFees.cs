namespace costs.net.integration.tests.Plugins.PG.TechnicalFees
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System.Threading.Tasks;

    public class IMEATechnicalFees : TechnicalFeeIntegrationTestBase
    {
        [Test]
        public async Task Tech_Fee_IMEA_Audio_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(115M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_UsageBuyout()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Buyout
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(115M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Digital()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Digital,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(450M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Photo_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(450M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Photo_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(115M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Video_CGI()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(115M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Video_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(450M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

        [Test]
        public async Task Tech_Fee_IMEA_Video_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.IndiaAndMiddleEastAfrica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(115M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("GBP");
        }

    }
}

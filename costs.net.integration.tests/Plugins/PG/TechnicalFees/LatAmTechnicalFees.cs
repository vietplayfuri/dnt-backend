namespace costs.net.integration.tests.Plugins.PG.TechnicalFees
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System.Threading.Tasks;

    public class LatAmTechnicalFees : TechnicalFeeIntegrationTestBase
    {
        [Test]
        public async Task Tech_Fee_LatAm_Audio_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(65M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_UsageBuyout()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Buyout
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(62M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Digital()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Digital,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(433M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Photo_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(433M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Photo_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(68M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Video_CGI()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(433M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Video_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(433M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_LatAm_Video_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Latim,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(103M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }
    }
}

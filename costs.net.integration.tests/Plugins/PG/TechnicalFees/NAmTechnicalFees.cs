namespace costs.net.integration.tests.Plugins.PG.TechnicalFees
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System.Threading.Tasks;

    public class NAmTechnicalFees : TechnicalFeeIntegrationTestBase
    {
        [Test]
        public async Task Tech_Fee_NorthAmerica_Audio_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(153M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_UsageBuyout()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Buyout
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(149M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Digital()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Digital,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(743M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Photo_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(743M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Photo_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(158M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Video_CGI()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(743M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Video_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(743M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }

        [Test]
        public async Task Tech_Fee_NorthAmerica_Video_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.NorthAmerica,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(215M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("USD");
        }
    }
}

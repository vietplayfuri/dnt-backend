namespace costs.net.integration.tests.Plugins.PG.TechnicalFees
{
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;

    public class EuropeTechnicalFees : TechnicalFeeIntegrationTestBase
    {
        [Test]
        public async Task Tech_Fee_Europe_Audio_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_UsageBuyout()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Digital()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Photo_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Photo_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Video_CGI()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Video_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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
        public async Task Tech_Fee_Europe_Video_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.Europe,
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

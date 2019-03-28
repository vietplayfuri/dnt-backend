namespace costs.net.integration.tests.Plugins.PG.TechnicalFees
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    public class ChinaTechnicalFees : TechnicalFeeIntegrationTestBase
    {
        [SetUp]
        public void Setup()
        { 
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
        }

        [Test]
        public async Task Tech_Fee_China_Audio_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Audio,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(392.06M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_UsageBuyout()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Buyout
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(392.06M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Digital()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Digital,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(0);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Photo_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(2736.55M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Photo_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Photography,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(392.06M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Video_CGI()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.CgiAnimation,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(2736.55M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Video_FP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.FullProduction,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(2736.55M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }

        [Test]
        public async Task Tech_Fee_China_Video_PP()
        {
            // Arrange
            InitData(
                budgetRegion: Constants.BudgetRegion.China,
                contentType: Constants.ContentType.Video,
                productionType: Constants.ProductionType.PostProductionOnly,
                costType: CostType.Production
            );

            // Act
            var receipt = await _technicalFeeService.GetTechnicalFee(_cost.Id);

            // Assert
            receipt.ConsultantRate.ShouldBeEquivalentTo(652M);
            receipt.CurrencyCode.ShouldBeEquivalentTo("HKD");
        }
    }
}

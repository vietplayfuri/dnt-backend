
namespace costs.net.plugins.tests.PG.Builders.Payments
{
    using System;
    using System.Collections.Generic;
    using core.Builders;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Builders.Payments;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public class PgCostSectionTotalsBuilderTests
    {
        private PgCostSectionTotalsBuilder _target;

        [SetUp]
        public void Setup()
        {
            _target = new PgCostSectionTotalsBuilder();
        }

        [Test]
        public void Null_StageForm_Returns_Empty()
        {
            //Arrange
            PgStageDetailsForm stageDetailsForm = null;
            var costLineItems = new List<CostLineItemView>();
            var stageKey = CostStages.OriginalEstimate.ToString();
            var expected = 0;

            //Act
            var result = _target.Build(stageDetailsForm, costLineItems, stageKey);

            //Assert
            result.Should().NotBeNull();
            result.InsuranceCostTotal.Should().Be(expected);
            result.OtherCostTotal.Should().Be(expected);
            result.PostProductionCostTotal.Should().Be(expected);
            result.ProductionCostTotal.Should().Be(expected);
            result.TargetBudgetTotal.Should().Be(expected);
            result.TechnicalFeeCostTotal.Should().Be(expected);
            result.TotalCostAmountTotal.Should().Be(expected);
        }

        [Test]
        public void Null_CostLineItemsStageForm_Returns_Empty()
        {
            //Arrange
            var stageDetailsForm = new PgStageDetailsForm();
            List<CostLineItemView> costLineItems = null;
            var stageKey = CostStages.OriginalEstimate.ToString();
            var expected = 0;

            //Act
            var result = _target.Build(stageDetailsForm, costLineItems, stageKey);

            //Assert
            result.Should().NotBeNull();
            result.InsuranceCostTotal.Should().Be(expected);
            result.OtherCostTotal.Should().Be(expected);
            result.PostProductionCostTotal.Should().Be(expected);
            result.ProductionCostTotal.Should().Be(expected);
            result.TargetBudgetTotal.Should().Be(expected);
            result.TechnicalFeeCostTotal.Should().Be(expected);
            result.TotalCostAmountTotal.Should().Be(expected);
        }

        [Test]
        public void Null_StageKey_Returns_Empty()
        {
            //Arrange
            var stageDetailsForm = new PgStageDetailsForm();
            var costLineItems = new List<CostLineItemView>();
            string stageKey = null;
            var expected = 0;

            //Act
            var result = _target.Build(stageDetailsForm, costLineItems, stageKey);

            //Assert
            result.Should().NotBeNull();
            result.InsuranceCostTotal.Should().Be(expected);
            result.OtherCostTotal.Should().Be(expected);
            result.PostProductionCostTotal.Should().Be(expected);
            result.ProductionCostTotal.Should().Be(expected);
            result.TargetBudgetTotal.Should().Be(expected);
            result.TechnicalFeeCostTotal.Should().Be(expected);
            result.TotalCostAmountTotal.Should().Be(expected);
        }

        [Test]
        public void Empty_StageKey_Returns_Empty()
        {
            //Arrange
            var stageDetailsForm = new PgStageDetailsForm();
            var costLineItems = new List<CostLineItemView>();
            var stageKey = string.Empty;
            var expected = 0;

            //Act
            var result = _target.Build(stageDetailsForm, costLineItems, stageKey);

            //Assert
            result.Should().NotBeNull();
            result.InsuranceCostTotal.Should().Be(expected);
            result.OtherCostTotal.Should().Be(expected);
            result.PostProductionCostTotal.Should().Be(expected);
            result.ProductionCostTotal.Should().Be(expected);
            result.TargetBudgetTotal.Should().Be(expected);
            result.TechnicalFeeCostTotal.Should().Be(expected);
            result.TotalCostAmountTotal.Should().Be(expected);
        }

        [Test]
        public void Video_PostProduction()
        {
            //Arrange
            var expectedInsurance = 10000;
            var expectedOtherCostTotal = 0;
            var expectedPostProductionCostTotal = 20000;
            var expectedProductionCostTotal = 0;
            var expectedTargetBudgetTotal = 0;
            var expectedTechnicalFee = 0;
            var expectedTotalCostAmount = 30000;

            var productionInsurance = 0;
            var postProductionInsurance = 10000;
            var technicalFee = 0;
            var postProduction = 20000;
            var production = 0;
            var initialBudget = 250000;

            var costLineItems = new List<CostLineItemView>
            {
                new CostLineItemView
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.CostSection.ProductionInsurance,
                    TemplateSectionName = Constants.CostSection.Production,
                    ValueInDefaultCurrency = productionInsurance
                },
                new CostLineItemView
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.CostSection.PostProductionInsurance,
                    TemplateSectionName = Constants.CostSection.PostProduction,
                    ValueInDefaultCurrency = postProductionInsurance
                },
                new CostLineItemView
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.CostSection.TechnicalFee,
                    ValueInDefaultCurrency = technicalFee
                },
                new CostLineItemView
                {
                    Id = Guid.NewGuid(),
                    TemplateSectionName = Constants.CostSection.PostProduction,
                    ValueInDefaultCurrency = postProduction
                },
                new CostLineItemView
                {
                    Id = Guid.NewGuid(),
                    TemplateSectionName = Constants.CostSection.Production,
                    ValueInDefaultCurrency = production
                }
            };
            var stageKey = CostStages.OriginalEstimate.ToString();
            var stageDetailsForm = new PgStageDetailsForm
            {
                AgencyCurrency = "USD",
                AgencyProducer = new[] { "Adine Becker (Leo Burnett)" },
                AgencyTrackingNumber = "123456",
                BudgetRegion = new AbstractTypeValue
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BudgetRegion.Europe,
                    Name = "Europe"
                },
                Campaign = "Test Campaign",
                ContentType = new DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.ContentType.Video,
                    Value = Constants.ContentType.Video
                },
                InitialBudget = initialBudget,
                IsAIPE = false,
                IsUsage = false,
                ProductionType = new DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.ProductionType.PostProductionOnly,
                    Value = Constants.ProductionType.PostProductionOnly
                },
                Title = "Test Cost"
            };

            //Act
            var result = _target.Build(stageDetailsForm, costLineItems, stageKey);

            //Assert
            result.Should().NotBeNull();
            result.InsuranceCostTotal.Should().Be(expectedInsurance);
            result.OtherCostTotal.Should().Be(expectedOtherCostTotal);
            result.PostProductionCostTotal.Should().Be(expectedPostProductionCostTotal);
            result.ProductionCostTotal.Should().Be(expectedProductionCostTotal);
            result.TargetBudgetTotal.Should().Be(expectedTargetBudgetTotal);
            result.TechnicalFeeCostTotal.Should().Be(expectedTechnicalFee);
            result.TotalCostAmountTotal.Should().Be(expectedTotalCostAmount);
        }
    }
}

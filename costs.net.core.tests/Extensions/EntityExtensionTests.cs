using System;
using costs.net.dataAccess.Entity;
using FluentAssertions;
using NUnit.Framework;

namespace costs.net.core.tests.Extensions
{
    using core.Extensions;

    [TestFixture]
    public class EntityExtensionTests
    {
        #region HasPreviousRevisions tests

        [Test]
        public void HasPreviousRevisions_HasRevisions()
        {
            //Arrange
            var cost = new Cost();
            var latestRevision = new CostStageRevision();
            var latestRevisionId = Guid.NewGuid();
            var previousRevision = new CostStageRevision();
            var previousRevisionId = Guid.NewGuid();
            var costStage = new CostStage();
            
            latestRevision.CostStage = costStage;
            latestRevision.Id = latestRevisionId;
            previousRevision.Id = previousRevisionId;

            costStage.CostStageRevisions.Add(latestRevision);
            costStage.CostStageRevisions.Add(previousRevision);

            cost.LatestCostStageRevision = latestRevision;

            //Act
            var result = cost.HasPreviousRevision();

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void HasPreviousRevisions_NoRevisions()
        {
            //Arrange
            var cost = new Cost();
            var latestRevision = new CostStageRevision();
            var latestRevisionId = Guid.NewGuid();
            var costStage = new CostStage();

            latestRevision.CostStage = costStage;
            latestRevision.Id = latestRevisionId;

            costStage.CostStageRevisions.Add(latestRevision);

            cost.LatestCostStageRevision = latestRevision;

            //Act
            var result = cost.HasPreviousRevision();

            //Assert
            result.Should().BeFalse();
        }

        #endregion // HasPreviousRevisions tests

        #region GetPreviousRevision tests

        [Test]
        public void GetPreviousRevision_HasOnePreviousRevision()
        {
            //Arrange
            var cost = new Cost();
            var latestRevision = new CostStageRevision();
            var latestRevisionId = Guid.NewGuid();
            var previousRevision = new CostStageRevision();
            var previousRevisionId = Guid.NewGuid();
            var costStage = new CostStage();

            latestRevision.CostStage = costStage;
            latestRevision.Id = latestRevisionId;
            latestRevision.Modified = DateTime.Now;
            latestRevision.Created = DateTime.Now;

            previousRevision.Id = previousRevisionId;
            previousRevision.Modified = DateTime.Now.AddSeconds(-1);
            previousRevision.Created = DateTime.Now.AddSeconds(-1);

            costStage.CostStageRevisions.Add(latestRevision);
            costStage.CostStageRevisions.Add(previousRevision);

            cost.LatestCostStageRevision = latestRevision;

            //Act
            var result = cost.GetPreviousRevision();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(previousRevision);
        }

        [Test]
        public void GetPreviousRevision_HasManyPreviousRevisions()
        {
            //Arrange
            var cost = new Cost();
            var latestRevision = new CostStageRevision();
            var latestRevisionId = Guid.NewGuid();
            var firstRevision = new CostStageRevision();
            var firstRevisionId = Guid.NewGuid();
            var secondRevision = new CostStageRevision();
            var secondRevisionId = Guid.NewGuid();
            var previousRevision = new CostStageRevision();
            var previousRevisionId = Guid.NewGuid();
            var costStage = new CostStage();

            latestRevision.CostStage = costStage;
            latestRevision.Id = latestRevisionId;
            latestRevision.Modified = DateTime.Now;
            latestRevision.Created = DateTime.Now;

            previousRevision.Id = previousRevisionId;
            previousRevision.Modified = DateTime.Now.AddSeconds(-1);
            previousRevision.Created = DateTime.Now.AddSeconds(-1);

            secondRevision.Id = secondRevisionId;
            secondRevision.Modified = DateTime.Now.AddSeconds(-2);
            secondRevision.Created = DateTime.Now.AddSeconds(-2);

            firstRevision.Id = firstRevisionId;
            firstRevision.Modified = DateTime.Now.AddSeconds(-3);
            firstRevision.Created = DateTime.Now.AddSeconds(-3);

            costStage.CostStageRevisions.Add(latestRevision);
            costStage.CostStageRevisions.Add(previousRevision);
            costStage.CostStageRevisions.Add(firstRevision);

            cost.LatestCostStageRevision = latestRevision;

            //Act
            var result = cost.GetPreviousRevision();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(previousRevision);
        }

        [Test]
        public void GetPreviousRevision_HasNoPreviousRevision()
        {
            //Arrange
            var cost = new Cost();
            var latestRevision = new CostStageRevision();
            var latestRevisionId = Guid.NewGuid();
            var costStage = new CostStage();

            latestRevision.CostStage = costStage;
            latestRevision.Id = latestRevisionId;
            latestRevision.Modified = DateTime.Now;
            latestRevision.Created = DateTime.Now;
            
            costStage.CostStageRevisions.Add(latestRevision);

            cost.LatestCostStageRevision = latestRevision;

            //Act
            var result = cost.GetPreviousRevision();

            //Assert
            result.Should().BeNull();
        }

        #endregion //GetPreviousRevision tests


        [Test]
        public void UpdateDefaultCurrencyValue_NullExchangeRate_DoesNothing()
        {
            //Arrange
            var cli = new CostLineItem();
            const decimal expectedDefaultCurrency = 0;
            const decimal expectedLocalCurrency = 0;
            cli.ValueInDefaultCurrency = expectedDefaultCurrency;
            cli.ValueInLocalCurrency = expectedLocalCurrency;

            //Act
            cli.UpdateDefaultCurrencyValue(null);

            //Assert
            cli.ValueInDefaultCurrency.Should().Be(expectedDefaultCurrency);
            cli.ValueInLocalCurrency.Should().Be(expectedLocalCurrency);
        }

        [Test]
        public void UpdateDefaultCurrencyValue_WithExchangeRate_CalculatesRate()
        {
            //Arrange
            var cli = new CostLineItem();
            const decimal expectedDefaultCurrency = 1.5M;
            const decimal expectedLocalCurrency = 1M;
            cli.ValueInDefaultCurrency = 0;
            cli.ValueInLocalCurrency = expectedLocalCurrency;
            var rate = new ExchangeRate
            {
                Rate = 1.5M
            };

            //Act
            cli.UpdateDefaultCurrencyValue(rate);

            //Assert
            cli.ValueInDefaultCurrency.Should().Be(expectedDefaultCurrency);
            cli.ValueInLocalCurrency.Should().Be(expectedLocalCurrency);
        }

        [Test]
        public void Within_Before_False()
        {
            var financialYear = new FinancialYear
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 12, 31)
            };
            var date = new DateTime(2000, 12, 31);

            var result = financialYear.Within(date);

            result.Should().BeFalse();
        }

        [Test]
        public void Within_After_False()
        {
            var financialYear = new FinancialYear
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 12, 31)
            };
            var date = new DateTime(2002, 01, 01);

            var result = financialYear.Within(date);

            result.Should().BeFalse();
        }

        [Test]
        public void Within_InBetween_True()
        {
            var financialYear = new FinancialYear
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 12, 31)
            };
            var date = new DateTime(2001, 01, 31);

            var result = financialYear.Within(date);

            result.Should().BeTrue();
        }
    }
}

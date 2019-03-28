
namespace costs.net.core.tests.Models.BillingExpenses
{
    using System;
    using core.Models.BillingExpenses;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class FinancialYearTests
    {
        [Test]
        public void IsPastYear_ReturnsTrue_ForPastYear()
        {
            //Arrange
            var start = new DateTime(2001, 01, 01);
            var end = new DateTime(2002, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsPastYear();

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsPastYear_ReturnsFalse_ForCurrentYear()
        {
            //Arrange
            var currentYear = DateTime.UtcNow.Year;
            var nextYear = currentYear + 1;
            var start = new DateTime(currentYear, 01, 01);
            var end = new DateTime(nextYear, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsPastYear();

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsPastYear_ReturnsFalse_ForFutureYear()
        {
            //Arrange
            var nextYear = DateTime.UtcNow.Year + 1;
            var yearAfterNext = nextYear + 1;
            var start = new DateTime(nextYear, 01, 01);
            var end = new DateTime(yearAfterNext, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsPastYear();

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsCurrentYear_ReturnsFalse_ForPastYear()
        {
            //Arrange
            var start = new DateTime(2001, 01, 01);
            var end = new DateTime(2002, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsCurrentYear();

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsCurrentYear_ReturnsTrue_ForCurrentYear()
        {
            //Arrange
            var currentYear = DateTime.UtcNow.Year;
            var nextYear = currentYear + 1;
            var start = new DateTime(currentYear, 01, 01);
            var end = new DateTime(nextYear, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsCurrentYear();

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsCurrentYear_ReturnsFalse_ForFutureYear()
        {
            //Arrange
            var nextYear = DateTime.UtcNow.Year + 1;
            var yearAfterNext = nextYear + 1;
            var start = new DateTime(nextYear, 01, 01);
            var end = new DateTime(yearAfterNext, 01, 01);
            var target = new FinancialYear
            {
                Start = start,
                End = end
            };

            //Act
            var result = target.IsCurrentYear();

            //Assert
            result.Should().BeFalse();
        }
    }
}

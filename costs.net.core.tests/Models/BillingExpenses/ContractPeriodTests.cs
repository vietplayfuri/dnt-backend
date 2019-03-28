
namespace costs.net.core.tests.Models.BillingExpenses
{
    using System;
    using core.Models.BillingExpenses;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ContractPeriodTests
    {
        [Test]
        public void Months_SameYear()
        {
            //Arrange
            const int expected = 12;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 12, 31)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_OneYearDiff()
        {
            //Arrange
            const int expected = 12;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2002, 01, 02)
            };           

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_MoreThanOneYearDiff()
        {
            //Arrange
            const int expected = 36;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 04, 01),
                End = new DateTime(2004, 03, 31)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_PartialMonth_LessThan15Days()
        {
            //Arrange
            const int expected = 0;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 01, 02)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_PartialMonth_15Days()
        {
            //Arrange
            const int expected = 1;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 01, 15)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_MoreThanOneYearDiff_MidMonth()
        {
            //Arrange
            const int expected = 20;
            var target = new ContractPeriod
            {
                Start = new DateTime(2018, 09, 18),
                End = new DateTime(2020, 04, 29)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void Months_LessThanOneYearDiff_MidMonth()
        {
            //Arrange
            const int expected = 10;
            var target = new ContractPeriod
            {
                Start = new DateTime(2018, 09, 18),
                End = new DateTime(2019, 06, 30)
            };

            //Act
            var result = target.TotalMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_SameYear()
        {
            //Arrange
            const int expected = 12;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 12, 31)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_OneYearDiff()
        {
            //Arrange
            const int expected = 12;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2002, 01, 02)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_MoreThanOneYearDiff()
        {
            //Arrange
            const int expected = 31;
            var target = new ContractPeriod
            {
                Start = new DateTime(2016, 11, 15),
                End = new DateTime(2019, 05, 27)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_SameMonth()
        {
            //Arrange
            const int expected = 0;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 01),
                End = new DateTime(2001, 01, 02)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_NextMonth()
        {
            //Arrange
            const int expected = 1;
            var target = new ContractPeriod
            {
                Start = new DateTime(2001, 01, 12),
                End = new DateTime(2001, 02, 07)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_MoreThanOneYearDiff_MidMonth()
        {
            //Arrange
            const int expected = 19;
            var target = new ContractPeriod
            {
                Start = new DateTime(2018, 09, 18),
                End = new DateTime(2020, 04, 29)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }

        [Test]
        public void PartialMonths_LessThanOneYearDiff_MidMonth()
        {
            //Arrange
            const int expected = 9;
            var target = new ContractPeriod
            {
                Start = new DateTime(2018, 09, 18),
                End = new DateTime(2019, 06, 30)
            };

            //Act
            var result = target.TotalPartialMonths;

            //Assert
            result.Should().Be(expected);
        }
    }
}

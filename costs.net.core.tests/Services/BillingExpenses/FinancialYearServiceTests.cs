namespace costs.net.core.tests.Services.BillingExpenses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.BillingExpenses;
    using dataAccess;
    using FluentAssertions;
    using core.Models;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using FinancialYear = core.Models.BillingExpenses.FinancialYear;

    [TestFixture]
    public class FinancialYearServiceTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();
        private FinancialYearService _target;

        [SetUp]
        public void Setup()
        {
            _target = new FinancialYearService(_efContextMock.Object);

            var financialYears = new List<dataAccess.Entity.FinancialYear>();
            for (int i = 0; i < 10; i++)
            {
                var start = new DateTime(2010 + i, 04, 01);
                var end = new DateTime(2011 + i, 03, 31);
                var fy = new dataAccess.Entity.FinancialYear
                {
                    ClientId = ClientType.Pg,
                    Start = start,
                    End = end
                };
                financialYears.Add(fy);
            }
            _efContextMock.MockAsyncQueryable(financialYears.AsQueryable(), d => d.FinancialYear);
        }

        [Test]
        public async Task EndDate_Before_StartDate_ReturnsFailure()
        {
            //Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddYears(-1);

            //Act
            var result = await _target.Calculate(BuType.Pg, startDate, endDate);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task EndDate_SameAs_StartDate_ReturnsSuccess()
        {
            //Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //Act
            var result = await _target.Calculate(BuType.Pg, startDate, endDate);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task EndDate_After_StartDate_ReturnsSuccess()
        {
            //Arrange
            var startDate = new DateTime(2017, 01, 01);
            var endDate = new DateTime(2019, 01, 01);
            var financialYearStart = new FinancialYear
            {
                ShortName = "2016/2017",
                Months = 3
            };
            var financialYearEnd = new FinancialYear
            {
                ShortName = "2018/2019",
                Months = 9
            };

            //Act
            var serviceResult = await _target.Calculate(BuType.Pg, startDate, endDate);

            //Assert
            serviceResult.Should().NotBeNull();
            serviceResult.Result.Should().NotBeNull();
            serviceResult.Result.First().Should().Be(financialYearStart);
            serviceResult.Result.Last().Should().Be(financialYearEnd);
        }
    }
}

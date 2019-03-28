namespace costs.net.core.tests.Services.Currencies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using core.Services.Currencies;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using core.Models;
    using core.Models.Currencies;
    using core.Models.User;
    using core.Services.ActivityLog;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using System.Threading;
    using System.Globalization;

    public class ExchangeRateServiceTests
    {
        private Mock<EFContext> _dbContext;
        private IExchangeRateService _service;
        private UserIdentity _user;
        private Mock<IActivityLogService> _activtyLogService;

        [SetUp]
        public void Setup()
        {
            _dbContext = new Mock<EFContext>();
            _activtyLogService = new Mock<IActivityLogService>();

            _user = new UserIdentity
            {
                FullName = "John Doe",
                Id = Guid.NewGuid()
            };
            _service = new ExchangeRateService(_dbContext.Object,
                new[]
                {
                    new Lazy<IExchangeRateBuilder, PluginMetadata>(
                        () => new Mock<IExchangeRateBuilder>().Object, new PluginMetadata { BuType =  BuType.Pg })
                },
                _activtyLogService.Object);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
        }

        [Test]
        public async Task CreateExchangeRateTest()
        {
            var defaultCurrency = new Currency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                DefaultCurrency = true,
                Description = "USD1",
                Symbol = "$"
            };

            var createModel = new CreateExchangeRateModel
            {
                CurrencyId = Guid.NewGuid(),
                Rate = decimal.Parse("0.73")
            };
            var data = new List<Currency> { defaultCurrency };
            _dbContext.MockAsyncQueryable(data.AsQueryable(), c => c.Currency);

            _dbContext.MockAsyncQueryable(new List<ExchangeRate>().AsQueryable(), c => c.ExchangeRate);

            var result = await _service.CreateExchangeRate(createModel, _user);

            result.ToCurrency.Should().Be(defaultCurrency.Id);
        }

        [Test]
        public void CreateExchangeRateButNoDefaultCurrencyTest()
        {
            _dbContext.MockAsyncQueryable(new List<Currency>().AsQueryable(), c => c.Currency);

            Assert.ThrowsAsync(typeof(Exception),
                () => _service.CreateExchangeRate(new CreateExchangeRateModel
                {
                    CurrencyId = Guid.NewGuid(),
                    Rate = decimal.Parse("0.73")
                }, _user));
        }

        [Test]
        public async Task CreateExchangeRatesTest()
        {
            var currencies =
                new[]
                {
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "USD",
                        Description = "US Dollar",
                        Symbol = "$",
                        DefaultCurrency = true
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "AUD",
                        Description = "Australian Dollar",
                        Symbol = "$"
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "CAD",
                        Description = "Canadian Dollar",
                        Symbol = "$"
                    }
                };

            var effectiveFrom = DateTime.UtcNow;

            var exchangeRates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    FromCurrency = currencies[1].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = effectiveFrom,
                    Rate = decimal.Parse("1.31")
                },
                new ExchangeRate
                {
                    FromCurrency = currencies[2].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = effectiveFrom.AddDays(-20),
                    Rate = decimal.Parse("0.715")
                },
                new ExchangeRate
                {
                    FromCurrency = currencies[2].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = effectiveFrom.AddDays(-11),
                    Rate = decimal.Parse("0.72")
                },
                new ExchangeRate
                {
                    FromCurrency = currencies[2].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = effectiveFrom.AddDays(-4),
                    Rate = decimal.Parse("0.719")
                },
                new ExchangeRate
                {
                    FromCurrency = currencies[2].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = effectiveFrom.AddDays(-2),
                    Rate = decimal.Parse("0.73926")
                }
            };

            _dbContext.MockAsyncQueryable(exchangeRates.AsQueryable(), c => c.ExchangeRate);
            _dbContext.MockAsyncQueryable(currencies.AsQueryable(), c => c.Currency);

            //_dbContext.Setup(m => m.SelectAsync(It.IsAny<Expression<Func<Currency, bool>>>())).ReturnsAsync(new[] { currencies[0] });

            var result = await _service.GetCurrentRates();

            var cadExRate = result.Single(x => x.FromCurrency == currencies[2].Id);
            cadExRate.Rate.Should().Be(decimal.Parse("0.73926"));
            cadExRate.EffectiveFrom.Should().Be(effectiveFrom.AddDays(-2));
        }

        [Test]
        public async Task GetPastMonthRates()
        {
            var pastDate = DateTime.Now.AddMonths(-3);

            var currencies = new[]
            {
                new Currency
                {
                    Id = Guid.NewGuid(),
                    Code = "USD",
                    Description = "US Dollar",
                    Symbol = "$",
                    DefaultCurrency = true
                },
                new Currency
                {
                    Id = Guid.NewGuid(),
                    Code = "AUD",
                    Description = "Australian Dollar",
                    Symbol = "$"
                },
                new Currency
                {
                    Id = Guid.NewGuid(),
                    Code = "CAD",
                    Description = "Canadian Dollar",
                    Symbol = "$"
                }
            };

            var exchangeRates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    FromCurrency = currencies[1].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = DateTime.UtcNow,
                    Rate = decimal.Parse("1.31")
                },
                new ExchangeRate
                {
                    FromCurrency = currencies[2].Id,
                    ToCurrency = currencies[0].Id,
                    EffectiveFrom = pastDate,
                    Rate = decimal.Parse("0.715")
                }
            };

            _dbContext.MockAsyncQueryable(exchangeRates.AsQueryable(), c => c.ExchangeRate);
            _dbContext.MockAsyncQueryable(currencies.AsQueryable(), c => c.Currency);

            var result = await _service.GetPastRates(pastDate);

            var cadExRate = result.Single(x => x.FromCurrency == currencies[2].Id);
            cadExRate.Rate.Should().Be(decimal.Parse("0.715"));
            cadExRate.EffectiveFrom.Should().Be(pastDate);
        }
    }
}

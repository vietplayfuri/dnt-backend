namespace costs.net.core.tests.Services.Currencies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using core.Services.Currencies;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using core.Models.Currencies;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    public class CurrencyServiceTests
    {
        private Mock<EFContext> _efContextMock;
        private ICurrencyService _service;
        [SetUp]
        public void Setup()
        {
            _efContextMock = new Mock<EFContext>();
   
            _service = new CurrencyService(_efContextMock.Object);
        }

        private static Currency GetDefaultCurrency()
        {
            return new Currency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Description = "US Dollar",
                Symbol = "$",
                DefaultCurrency = true
            };
        }

        [Test]
        public async Task ListCurrenciesTest()
        {
            var currencies =
                new[]
                {
                    GetDefaultCurrency(),
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "AUD",
                        Description = "Australian Dollar",
                        Symbol = "$"
                    }
                };

            _efContextMock.MockAsyncQueryable(currencies.AsQueryable(), d => d.Currency);

            var model = await _service.ListAggregateCurrencies();

            model.Count.Should().Be(2);

            model.Single(x => x.Code == "USD").DefaultCurrency.Should().BeTrue();

            var audCurrency = model.Single(x => x.Code == "AUD");
            audCurrency.Should().Be(currencies[1]);
        }

        [Test]
        public async Task CreateMultipleTest()
        {
            var currencies =
                new[]
                {
                    GetDefaultCurrency(),
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


            _efContextMock.MockAsyncQueryable(currencies.AsQueryable(), d => d.Currency);

            var model = await _service.ListAggregateCurrencies();

            model.Count.Should().Be(3);

            var cadCurrency = model.Single(x => x.Code == "CAD");

            cadCurrency.Should().Be(currencies[2]);
        }

        [Test]
        public async Task CreateCurrencyTest()
        {
            var defaultCurrency = GetDefaultCurrency();
            _efContextMock
                .MockAsyncQueryable(new []{ defaultCurrency }.AsQueryable(), d => d.Currency)
                .Setup(c => c.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultCurrency);

            var model = new Currency
            {
                Code = "CAD",
                Description = "Canadian Dollar",
                Symbol = "$",
            };

            await _service.CreateCurrency(model);

            //TODO: this test should be in the tests for dbContext: result.Id.Should().Be(newId); 

            _efContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CreateDefaultCurrencyTest()
        {
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Description = "US Dollar",
                Symbol = "$",
                DefaultCurrency = false
            };

            _efContextMock
                .MockAsyncQueryable(new[] { currency }.AsQueryable(), d => d.Currency)
                .Setup(c => c.FindAsync(currency.Id))
                .ReturnsAsync(currency);

            await _service.SetDefaultCurrency(currency.Id);

            _efContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            currency.DefaultCurrency.Should().Be(true);
        }

        [Test]
        public async Task GetCurrencyByIdTest()
        {
            var currency = GetDefaultCurrency();
            _efContextMock
                .MockAsyncQueryable(new[] { currency }.AsQueryable(), d => d.Currency)
                .Setup(c => c.FindAsync(currency.Id))
                .ReturnsAsync(currency);

            var model = await _service.GetCurrency(currency.Id);

            model.Should().Be(currency);
        }

        [Test]
        public async Task GetCurrencyByIdMissingTest()
        {
            var id = Guid.NewGuid();
            var currencies = new List<Currency>(){new Currency()}.AsQueryable();
            _efContextMock
                .MockAsyncQueryable(currencies, d => d.Currency);

            var result = await _service.GetCurrency(id);

            result.Should().BeNull();
        }

        [Test]
        public async Task UpdateCurrencyTest()
        {
            var currency = GetDefaultCurrency();
            _efContextMock
                .MockAsyncQueryable(new [] { currency }.AsQueryable(), d => d.Currency)
                .Setup(c => c.FindAsync(currency.Id))
                .ReturnsAsync(currency);

            var model = new UpdateCurrencyModel
            {
                Code = "AUD",
                Description = "Australian Dollar",
                Symbol = "$"
            };

            await _service.UpdateCurrency(currency.Id, model);

            _efContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SetDefaultCurrencyTest()
        {
            var defaultCurrency = GetDefaultCurrency();
            defaultCurrency.DefaultCurrency = false;

            _efContextMock
                .MockAsyncQueryable(new[] { defaultCurrency }.AsQueryable(), d => d.Currency)
                .Setup(c => c.FindAsync(defaultCurrency.Id))
                .ReturnsAsync(defaultCurrency);

            await _service.SetDefaultCurrency(defaultCurrency.Id);

            defaultCurrency.DefaultCurrency.Should().BeTrue();

            _efContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
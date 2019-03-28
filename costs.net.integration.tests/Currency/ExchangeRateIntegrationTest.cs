namespace costs.net.integration.tests.Currency
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Browser;
    using core.Models.ACL;
    using core.Models.Currencies;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;

    public class ExchangeRateIntegrationTest : BaseIntegrationTest
    {
        private CostUser _user;

        [SetUp]
        public async Task SetUp()
        {
            _user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
        }

        private async Task<Currency> CreateCurrency(string code, string description, string symbol)
        {
            return Deserialize<Currency>(await Browser.Post("/v1/currency", w =>
            {
                w.User(_user);
                w.JsonBody(new CreateCurrencyModel
                {
                    Code = code,
                    Description = description,
                    Symbol = symbol
                });
            }), HttpStatusCode.Created);
        }

        [Test]
        public async Task A01_CreateInvalidCurrencyWhenMissingDefaultTest()
        {
            // set default currency to false
            var currencies = await EFContext.Currency.Where(c => c.DefaultCurrency).ToArrayAsync();
            foreach (var c in currencies)
            {
                c.DefaultCurrency = false;
            }
            await EFContext.SaveChangesAsync();

            var currency = await CreateCurrency("CAD", "Canadian Dollar", "$");

            var model = new CreateExchangeRateModel
            {
                CurrencyId = currency.Id,
                Rate = decimal.Parse("0.73")
            };

            var createResult = await Browser.Post("/v1/exchangerate/create", with =>
            {
                with.User(_user);
                with.JsonBody(model);
            });

            createResult.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task CreateExchangeRatesTest()
        {
            // Create default currency
            var defaultCurrency = await CreateCurrency("USD", "United States Dollar", "$");

            ValidateStatusCode(
                await Browser.Put($"/v1/currency/{defaultCurrency.Id}/default", w => w.User(_user)),
                HttpStatusCode.OK);

            var audCurrency = await CreateCurrency("AUD", "Australian Dollar", "$");

            var cadCurrency = await CreateCurrency("CAD", "Canadian Dollar", "$");

            var effectiveFrom = DateTime.UtcNow;

            var exchangeRates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    FromCurrency = audCurrency.Id,
                    ToCurrency = defaultCurrency.Id,
                    EffectiveFrom = effectiveFrom,
                    RateName = "Rate 1",
                    Rate = decimal.Parse("1.31")
                },
                new ExchangeRate
                {
                    FromCurrency = cadCurrency.Id,
                    ToCurrency = defaultCurrency.Id,
                    RateName = "Rate 2",
                    Rate = decimal.Parse("0.715")
                },
                new ExchangeRate
                {
                    FromCurrency = cadCurrency.Id,
                    ToCurrency = defaultCurrency.Id,
                    RateName = "Rate 3",
                    Rate = decimal.Parse("0.72")
                },
                new ExchangeRate
                {
                    FromCurrency = cadCurrency.Id,
                    ToCurrency = defaultCurrency.Id,
                    RateName = "Rate 4",
                    Rate = decimal.Parse("0.719")
                },
                new ExchangeRate
                {
                    FromCurrency = cadCurrency.Id,
                    ToCurrency = defaultCurrency.Id,
                    RateName = "Rate 5",
                    Rate = decimal.Parse("0.73926")
                }
            };

            foreach (var exchangeRate in exchangeRates)
            {
                ValidateStatusCode(await Browser.Post("/v1/exchangerate/create", w =>
                {
                    w.User(_user);
                    w.JsonBody(new CreateExchangeRateModel
                    {
                        CurrencyId = exchangeRate.FromCurrency,
                        Rate = exchangeRate.Rate
                    });
                }), HttpStatusCode.Created);
            }

            var listResult = await Browser.Get("/v1/exchangerate", w => w.User(_user));
            var list = Deserialize<List<ExchangeRate>>(listResult, HttpStatusCode.OK);

            Assert.AreEqual(1.31, list.Single(x => x.FromCurrency == audCurrency.Id).Rate);
            Assert.AreEqual(0.73926, list.Single(x => x.FromCurrency == cadCurrency.Id).Rate);
        }

        [Test]
        public async Task GetExchangeRateForCurrencyTest()
        {
            // Create default currency
            var defaultCurrency = await CreateCurrency("USD", "United States Dollar", "$");

            await Browser.Put($"/v1/currency/default/{defaultCurrency.Id}", w => w.User(_user));

            var audCurrency = await CreateCurrency("AUD", "Australian Dollar", "$");

            var audExchangeRateResult = await Browser.Post("/v1/exchangerate/create",
                w => {
                    w.User(_user);
                    w.JsonBody(new CreateExchangeRateModel
                    {
                        CurrencyId = audCurrency.Id,
                        Rate = decimal.Parse("0.44")
                    });
                });
            audExchangeRateResult.StatusCode.Should().Be(HttpStatusCode.Created);

            var getCurrencyExchangeRateResult = await Browser
                .Get($"/v1/currency/{audCurrency.Id}/exchangerate", w => w.User(_user));
            getCurrencyExchangeRateResult.StatusCode.Should().Be(HttpStatusCode.OK);

            var audExchangeRate = Deserialize<ExchangeRate>(getCurrencyExchangeRateResult, HttpStatusCode.OK);
            audExchangeRate.Rate.Should().Be(decimal.Parse("0.44"));
        }

        [Test]
        public async Task GetAllExchangeRatesForCurrencyTest()
        {
            // Create default currency
            var defaultCurrency = await CreateCurrency("USD", "United States Dollar", "$");

            await Browser.Put($"/v1/currency/default/{defaultCurrency.Id}");

            var audCurrency = await CreateCurrency("AUD", "Australian Dollar", "$");

            var createModels = new List<CreateExchangeRateModel>
            {
                new CreateExchangeRateModel
                {
                    CurrencyId = audCurrency.Id,
                    Rate = decimal.Parse("0.44")
                },
                new CreateExchangeRateModel
                {
                    CurrencyId = audCurrency.Id,
                    Rate = decimal.Parse("0.442")
                },
                new CreateExchangeRateModel
                {
                    CurrencyId = audCurrency.Id,
                    Rate = decimal.Parse("0.447")
                },
                new CreateExchangeRateModel
                {
                    CurrencyId = audCurrency.Id,
                    Rate = decimal.Parse("0.501")
                },
                new CreateExchangeRateModel
                {
                    CurrencyId = audCurrency.Id,
                    Rate = decimal.Parse("0.4992")
                }
            };

            foreach (var createExchangeRateModel in createModels)
            {
                var audExchangeRateResult = await Browser.Post("/v1/exchangerate/create", w =>
                {
                    w.User(_user);
                    w.JsonBody(createExchangeRateModel);
                });
                audExchangeRateResult.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            var getCurrencyExchangeRateResult = await Browser
                .Get($"/v1/currency/{audCurrency.Id}/exchangerate/list", w =>
                {
                    w.User(_user);
                });

            var audExchangeRates = Deserialize<List<ExchangeRate>>(getCurrencyExchangeRateResult, HttpStatusCode.OK);

            audExchangeRates.Count.Should().Be(5);
            audExchangeRates[4].Rate.Should().Be(decimal.Parse("0.4992"));
        }

        [Test]
        public async Task ExchangeRateModule_ProcessExchangeRatesCSV_Correctly()
        {
            var csvFile = @"Currency Name,Code,,29-Feb,Exch Rate,Abstract Type Id,,,,,,
                            BULGARIA,BGN,B/S,0.55903,0.55903,,,,,,,
                            CANADA,CAD,B / S,0.73926,0.73926,,,,,,,";
            var url = "/v1/exchangerate/uploadcsv";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvFile));
            var browserContext = new BrowserContextMultipartFormData(c =>
            {
                c.AddFile("file", "file.csv", "text/csv", stream);
            });

            var result = await Browser.Post(url, c =>
            {
                c.User(_user);
                c.MultiPartFormData(browserContext);
            });

            ValidateStatusCode(result, HttpStatusCode.OK);
        }

        [Test]
        public async Task ExchangeRateModule_UploadNoCSV_ReturnsBadRequest()
        {
            var browserContext = new BrowserContextMultipartFormData(c =>
            {
            });
            var url = "/v1/exchangerate/uploadcsv";

            var result = await Browser.Post(url, c =>
            {
                c.User(_user);
                c.MultiPartFormData(browserContext);
            });

            ValidateStatusCode(result, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task ExchangeRateModule_DownloadCSV_Correctly()
        {
            // Create default currency
            var defaultCurrency = await CreateCurrency("USD", "United States Dollar", "$");
            await Browser.Put($"/v1/currency/default/{defaultCurrency.Id}", w => w.User(_user));

            const string url = "/v1/exchangerate/downloadcsv";

            var result = await Browser.Get(url, c =>
            {
                c.User(_user);
            });

            ValidateStatusCode(result, HttpStatusCode.OK);

            result.ContentType.Should().Be("text/csv");
        }
    }
}
namespace costs.net.plugins.PG.Builders
{
    using core.Builders;
    using CsvHelper;
    using dataAccess.Entity;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Common;
    using Models;

    public class PgExchangeRateBuilder : IExchangeRateBuilder
    {
        private IEnumerable<Currency> Currencies;
                                                 
        public IEnumerable<IExchangeRateDetails> GetExchangeRatesFromCsv(IEnumerable<Stream> files, IEnumerable<Currency> currencies)
        {
            foreach (var file in files)
            {
                PgExchangeRateDetails exchangeRateDetails = null;

                using (var fileStream = file)
                {
                    using (var textReader = new StreamReader(fileStream))
                    {
                        using (var csvHelper = new CsvReader(textReader))
                        {
                            while (csvHelper.Read())
                            {
                                var exchangeRateName = csvHelper.GetField<string>(0).Trim();
                                var currencyCode = csvHelper.GetField<string>(1).Trim();
                                var rateType = csvHelper.GetField<string>(2).Trim();
                                var oldExchangeRate = csvHelper.GetField<decimal?>(3);
                                var exchangeRate = csvHelper.GetField<decimal?>(4);
                                Currency currency = currencies.LastOrDefault(a => a.Code == currencyCode);

                                if (string.IsNullOrEmpty(currencyCode)
                                    || !exchangeRate.HasValue
                                    || currency == null)
                                {
                                    //if (_log.IsWarnEnabled)
                                    //{
                                    //    _log.Warn($"Currency with code {currencyCode} not found.");
                                    //}
                                    continue;
                                }

                                exchangeRateDetails = new PgExchangeRateDetails
                                {
                                    CurrencyId = currency.Id,
                                    RateName = exchangeRateName,
                                    OldRate = oldExchangeRate,
                                    CurrencyCode = currencyCode,
                                    Rate = exchangeRate.Value,
                                    RateType = rateType
                                };
                            }
                        }
                    }
                }

                if (exchangeRateDetails != null)
                {
                    yield return exchangeRateDetails;
                }
            }
        }

        public async Task<byte[]> CreateCsvFileFromExchangeRates(IEnumerable<IExchangeRateDetails> exchangeRates)
        {
            using (var stream = new MemoryStream())
            {
                using (var streamWritter = new StreamWriter(stream))
                {
                    using (var csvWriter = new CsvWriter(streamWritter))
                    {
                        csvWriter.WriteField("Currency Name");
                        csvWriter.WriteField("Code");
                        csvWriter.WriteField("Type");
                        csvWriter.WriteField("Previous Exchange Rate");
                        csvWriter.WriteField("Current Exchange Rate");
                        csvWriter.NextRecord();

                        foreach (var exchangeRate in exchangeRates)
                        {
                            csvWriter.WriteField(exchangeRate.RateName);
                            csvWriter.WriteField(exchangeRate.CurrencyCode);
                            csvWriter.WriteField(exchangeRate.RateType);
                            csvWriter.WriteField(exchangeRate.OldRate);
                            csvWriter.WriteField(exchangeRate.Rate);
                            csvWriter.NextRecord();
                        }
                        await streamWritter.FlushAsync();
                        return stream.ToArray();
                    }
                }
            }
        }

        public IEnumerable<IExchangeRateDetails> GetExchangeRatesFromCsv(IEnumerable<IFormFile> files, IEnumerable<Currency> currencies)
        {
            var listResult = new List<PgExchangeRateDetails>();
            foreach (var file in files)
            {
                using (var fileStream = file.OpenReadStream())
                {
                    using (var textReader = new StreamReader(fileStream))
                    {
                        using (var csvHelper = new CsvReader(textReader))
                        {
                            csvHelper.Read();
                            while (csvHelper.Read())
                            {
                                var exchangeRateName = csvHelper.GetField<string>(0).Trim();
                                var currencyCode = csvHelper.GetField<string>(1).Trim();
                                var rateType = csvHelper.GetField<string>(2).Trim();
                                var oldExchangeRate = csvHelper.GetField<decimal?>(3);
                                var exchangeRate = csvHelper.GetField<decimal?>(4);
                                Currency currency = currencies.LastOrDefault(a => a.Code == currencyCode);

                                if (string.IsNullOrEmpty(currencyCode)
                                    || !exchangeRate.HasValue
                                    || currency == null)
                                {
                                    //if (_log.IsWarnEnabled)
                                    //{
                                    //    _log.Warn($"Currency with code {currencyCode} not found.");
                                    //}
                                    continue;
                                }

                                var exchangeRateDetails = new PgExchangeRateDetails
                                {
                                    CurrencyId = currency.Id,
                                    RateName = exchangeRateName,
                                    OldRate = oldExchangeRate,
                                    CurrencyCode = currencyCode,
                                    Rate = exchangeRate.Value,
                                    RateType = rateType
                                };

                                listResult.Add(exchangeRateDetails);

                            }
                        }
                    }
                }

            }

            return listResult;
        }

        public IExchangeRateDetails GetExchangeRateDetails(ExchangeRate exchangeRate)
        {
            return new PgExchangeRateDetails
            {
                RateType = exchangeRate.RateType,
                CurrencyId = exchangeRate.FromCurrency,
                OldRate = exchangeRate.OldRate,
                Rate = exchangeRate.Rate,
                RateName = exchangeRate.RateName
            };
        }

        public void SetCurrencies(IEnumerable<Currency> currencies)
        {
            Currencies = currencies;
        }

        public IExchangeRateDetails GetExchangeRateDetailsWithCode(ExchangeRate exchangeRate)
        {
            return new PgExchangeRateDetails
            {
                CurrencyId = exchangeRate.FromCurrency,
                RateName = exchangeRate.RateName,
                OldRate = exchangeRate.OldRate,
                CurrencyCode = (Currencies != null) ? Currencies.FirstOrDefault(a => a.Id == exchangeRate.FromCurrency).Code : null,
                Rate = exchangeRate.Rate,
                RateType = exchangeRate.RateType
            };
        }
    }
}
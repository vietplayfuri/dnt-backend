using System;
using costs.net.dataAccess.Entity;
using FluentAssertions;
using NUnit.Framework;

namespace costs.net.core.tests.Extensions
{
    using core.Extensions;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class CostModelExtensionsTest
    {
        [Test]
        [TestCase("British Pound", "Euro", 1000, 25000, 2000)]
        [TestCase("US Dollar", "US Dollar", 10000, 25000, 2000)]
        public void GetRevisionTotals_For_Different_Cost_Section_Currencies(string defaultRateName, string exchangeRateName, decimal costLineOne, decimal costLineTwo, decimal costLineThree)
        {
            // Arrange
            var data = SetupCurrencies();
            var defaultExchangeRate = data.Item2.First(a => a.RateName == defaultRateName);
            var costLineExchangeRate = data.Item2.First(a => a.RateName == exchangeRateName);

            var cost = AddCostWithRevisions(defaultExchangeRate, costLineExchangeRate, costLineOne, costLineTwo, costLineThree);

            // Act
            var result = cost.LatestCostStageRevision.GetRevisionTotals(data.Item2, defaultExchangeRate.FromCurrency);

            // Assert
            var total = costLineOne + costLineTwo + costLineThree;

            // Always starts with USD by default so to get exchange rate its a simple division to get local (Agency default) currency
            var totalLocalCurrency = total / defaultExchangeRate.Rate;

            result.total.Should().Be(total);
            result.totalInLocalCurrency.Should().Be(totalLocalCurrency);
        }

        private (List<Currency>, List<ExchangeRate>) SetupCurrencies()
        {
            var gbpId = Guid.NewGuid();
            var eurId = Guid.NewGuid();
            var usdId = Guid.NewGuid();

            var gbp = new Currency
            {
                Code = "GBP",
                DefaultCurrency = false,
                Id = gbpId
            };
            var usd = new Currency
            {
                Code = "USD",
                DefaultCurrency = true,
                Id = usdId
            };
            var eur = new Currency
            {
                Code = "EUR",
                DefaultCurrency = false,
                Id = eurId
            };
            var currencies = new List<Currency> { gbp, usd, eur };
            var gbpExchangeRate = new ExchangeRate
            {
                FromCurrency = gbpId,
                ToCurrency = usdId,
                Rate = 4m,
                RateName = "British Pound",
                EffectiveFrom = DateTime.Now.AddMonths(-3)
            };
            var eurExchangeRate = new ExchangeRate
            {
                FromCurrency = eurId,
                ToCurrency = usdId,
                Rate = 2m,
                RateName = "Euro",
                EffectiveFrom = DateTime.Now.AddMonths(-3)
            };
            var dorExchangeRate = new ExchangeRate
            {
                FromCurrency = usdId,
                ToCurrency = usdId,
                Rate = 1m,
                RateName = "US Dollar",
                EffectiveFrom = DateTime.Now.AddMonths(-3)
            };

            var defaultExchangeRates = new List<ExchangeRate> { gbpExchangeRate, eurExchangeRate, dorExchangeRate };
            return (currencies, defaultExchangeRates);
            //_efContext.Currency.AddRange(currencies);
            //_efContext.ExchangeRate.AddRange(defaultExchangeRates);
            //_efContext.SaveChanges();
        }

        private Cost AddCostWithRevisions(ExchangeRate defaultExchangeRate, ExchangeRate costLineExchangeRate, decimal costLineOne, decimal costLineTwo, decimal costLineThree)
        {
            Guid costId = Guid.NewGuid();

            var latestCostStageRevision = new CostStageRevision()
            {
                Approvals = new List<Approval>(),
                BillingExpenses = new List<BillingExpense>(),
                AssociatedAssets = new List<AssociatedAsset>(),
                CostFormDetails = new List<CostFormDetails>(),
                CostLineItems = new List<CostLineItem>()
                {
                    new CostLineItem
                    {
                        LocalCurrencyId = costLineExchangeRate.FromCurrency,
                        ValueInDefaultCurrency = costLineOne,
                        ValueInLocalCurrency = (costLineOne / costLineExchangeRate.Rate),
                        Name = "offlineEdits"
                    },
                    new CostLineItem
                    {
                        LocalCurrencyId = costLineExchangeRate.FromCurrency,
                        ValueInDefaultCurrency = costLineTwo,
                        ValueInLocalCurrency = (costLineTwo / costLineExchangeRate.Rate),
                        Name = "pensionAndHealth"
                    },
                    new CostLineItem
                    {
                        LocalCurrencyId = costLineExchangeRate.FromCurrency,
                        ValueInDefaultCurrency = costLineThree,
                        ValueInLocalCurrency = (costLineThree / costLineExchangeRate.Rate),
                        Name = "audioFinalization"
                    }
                },
                CostStageRevisionPaymentTotals = new List<CostStageRevisionPaymentTotal>(),
                Created = DateTime.UtcNow,
                ExpectedAssets = new List<ExpectedAsset>(),
                CustomObjectData = new List<CustomObjectData>(),
                StageDetails = new CustomFormData(),
                Status = CostStageRevisionStatus.PendingTechnicalApproval,
                IsLineItemSectionCurrencyLocked = true,
                IsPaymentCurrencyLocked = true,
                Name = "FirstPresentation",
                Modified = DateTime.UtcNow,
                SupportingDocuments = new List<SupportingDocument>(),
                CostStage = new CostStage { Cost = new Cost { ExchangeRateDate = DateTime.UtcNow, } }
            };

            var cost = new Cost
            {
                Id = costId,
                CostStages = new List<CostStage>
                {
                    new CostStage
                    {
                        Name = "Original Estimate",
                        Key = "OrignalEstimate",
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        StageOrder = 1,
                        CostStageRevisions = new List<CostStageRevision>()
                        {
                            new CostStageRevision()
                            {
                                Approvals = new List<Approval>(),
                                BillingExpenses = new List<BillingExpense>(),
                                AssociatedAssets = new List<AssociatedAsset>(),
                                CostFormDetails = new List<CostFormDetails>(),
                                CostLineItems = new List<CostLineItem>(),
                                CostStageRevisionPaymentTotals = new List<CostStageRevisionPaymentTotal>(),
                                Created = DateTime.UtcNow,
                                ExpectedAssets = new List<ExpectedAsset>(),
                                CustomObjectData = new List<CustomObjectData>(),
                                StageDetails = new CustomFormData(),
                                Status = CostStageRevisionStatus.Approved,
                                IsLineItemSectionCurrencyLocked = true,
                                IsPaymentCurrencyLocked = true,
                                Name = "OriginalEstimate",
                                Modified = DateTime.UtcNow,
                                SupportingDocuments = new List<SupportingDocument>(),
                                CostStage = new CostStage { Cost = new Cost { ExchangeRateDate = DateTime.UtcNow, } }
                            }
                        }
                    },
                    new CostStage
                    {
                        Name = "First Presentation",
                        Key = "FirstPresentation",
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        StageOrder = 2,
                        CostStageRevisions = new List<CostStageRevision>
                        {
                            latestCostStageRevision
                        }
                    }
                },
                Parent = new AbstractType
                {
                    Agency = new Agency
                    { }
                },
                LatestCostStageRevision = latestCostStageRevision,
                CostNumber = "NUMBER011",
                ExchangeRateDate = DateTime.UtcNow,
                ExchangeRate = defaultExchangeRate.Rate,
                PaymentCurrencyId = defaultExchangeRate.FromCurrency,
            };

            return cost;
        }
    }
}

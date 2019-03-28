namespace costs.net.plugins.tests.PG.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Exception;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services.Costs;

    [TestFixture]
    public class PgCostServiceTests
    {
        private EFContext _efContext;
        private Mock<ICostStageRevisionService> _costStageRevisionServicee;
        private PgCostService _costService;
        Guid gbpId;
        Guid eurId;
        Guid usdId;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _costStageRevisionServicee = new Mock<ICostStageRevisionService>();
            _costService = new PgCostService(_efContext, _costStageRevisionServicee.Object);
            gbpId = Guid.NewGuid();
            eurId = Guid.NewGuid();
            usdId = Guid.NewGuid();
        }

        [Test]
        public void IsValidForSubmittion_WhenCostDoesNotExist_Should_ThrowException()
        {
            // Arrange
            var costId = Guid.NewGuid();

            // Act
            // Assert
            _costService.Awaiting(c => c.IsValidForSubmittion(costId)).ShouldThrow<EntityNotFoundException<Cost>>();
        }

        [Test]
        public async Task IsValidForSubmittion_When_CostIsNotDPV_And_SAPVendorLabelIsMissing_ShouldReturnFailure()
        {
            // Arrange
            var cost = AddCost(false, false);

            // Act
            var result = await _costService.IsValidForSubmittion(cost.Id);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task IsValidForSubmittion_When_CostIsNotDPV_And_HasSAPVendorLabel_ShouldReturnSuccess()
        {
            // Arrange
            var cost = AddCost(false, true);

            // Act
            var result = await _costService.IsValidForSubmittion(cost.Id);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task IsValidForSubmittion_When_CostIsDPV_And_SAPVendorLabelIsMissing_ShouldReturnFailure()
        {
            // Arrange
            var cost = AddCost(true, false);

            // Act
            var result = await _costService.IsValidForSubmittion(cost.Id);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task IsValidForSubmittion_When_CostIsDPV_And_HasSAPVendorLabel_ShouldReturnSuccess()
        {
            // Arrange
            var cost = AddCost(true, true);

            // Act
            var result = await _costService.IsValidForSubmittion(cost.Id);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Test]
        [TestCase("British Pound", "Euro", 1000, 25000, 2000)]
        [TestCase("US Dollar", "US Dollar", 10000, 25000, 2000)]
        public async Task GetRevisionTotals_For_Different_Cost_Section_Currencies(string defaultRateName, string exchangeRateName, decimal costLineOne, decimal costLineTwo, decimal costLineThree)
        {
            // Arrange
            SetupCurrencies();
            var defaultExchangeRate = _efContext.ExchangeRate.First(a => a.RateName == defaultRateName);
            var costLineExchangeRate = _efContext.ExchangeRate.First(a => a.RateName == exchangeRateName);

            var cost = AddCostWithRevisions(defaultExchangeRate, costLineExchangeRate, costLineOne, costLineTwo, costLineThree);

            // Act
            var result = await _costService.GetRevisionTotals(cost.LatestCostStageRevisionId.Value);

            // Assert
            var total = costLineOne + costLineTwo + costLineThree;

            // Always starts with USD by default so to get exchange rate its a simple division to get local (Agency default) currency
            var totalLocalCurrency = total / defaultExchangeRate.Rate;

            result.total.Should().Be(total);
            result.totalInLocalCurrency.Should().Be(totalLocalCurrency);
        }

        private Cost AddCost(bool isDpv, bool hasSapCode)
        {
            Guid costId = Guid.NewGuid();

            var cost = new Cost
            {
                Id = costId,
                Parent = new AbstractType
                {
                    Agency = new Agency
                    {
                        Labels = new[]
                        {
                            hasSapCode
                                ? $"{Constants.PurchaseOrder.VendorSapIdLabelPrefix}test agency vendor sap code"
                                : string.Empty
                        }
                    }
                },
                LatestCostStageRevision = new CostStageRevision
                {
                    ProductDetails = new CustomFormData
                    {
                        Data = JsonConvert.SerializeObject(new PgProductionDetailsForm
                        {
                            DirectPaymentVendor = isDpv
                                ? new PgProductionDetailsForm.Vendor
                                {
                                    SapVendorCode = hasSapCode ? "test DPV vendor sap code" : string.Empty
                                }
                                : null
                        })
                    }
                }
            };
            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            return cost;
        }

        private void SetupCurrencies()
        {
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
            _efContext.Currency.AddRange(currencies);
            _efContext.ExchangeRate.AddRange(defaultExchangeRates);
            _efContext.SaveChanges();
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
                SupportingDocuments = new List<SupportingDocument>()
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
                                SupportingDocuments = new List<SupportingDocument>()
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
            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            return cost;
        }


        #region public async Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(CostStageRevision revision)

        /// <summary>
        /// Note: default payment currency code always is USD
        /// </summary>
        [Test]
        [TestCase("USD", 1, 10000, "true", 2, 10000)]
        [TestCase("USD", 1, 10000, "false", 2, 10000)] /* 10K because currency of cost line item and default payment currency are the same */
        [TestCase("GBP", 1, 10000, "true", 2, 10000)]
        [TestCase("GBP", 1, 10000, "false", 2, 20000)]
        [TestCase("GBP", 1, 10000, "false", 3, 30000)]
        [TestCase("EUR", 1, 10000, "true", 2, 10000)]
        [TestCase("EUR", 1, 10000, "false", 2, 20000)]
        [TestCase("EUR", 1, 10000, "false", 3, 30000)]
        public async Task GetRevisionTotals(
            string costLineItemCurrencyCodeInThePast,
            decimal oldCurrencyRate,
            decimal firstCostItemAmount,
            string isSubmit,
            decimal newCurrencyRate,
            decimal expectedTotalAmountWithNewCurrencyRate)
        {
            // Arrange
            SetupCurrencies();
            var oldCostLineExchangeRate = SetupNewExchangeRate(costLineItemCurrencyCodeInThePast, oldCurrencyRate, DateTime.Now.AddMonths(-2));
            SetupNewExchangeRate(costLineItemCurrencyCodeInThePast, newCurrencyRate, DateTime.Now.AddMonths(-1));
            DateTime? exchangeRateDate = null;
            if (Boolean.Parse(isSubmit))
            {
                exchangeRateDate = DateTime.Now.AddMonths(-2); //same time with old currency rate date
            }

            var cost = AddCostLineItems(oldCostLineExchangeRate, exchangeRateDate, firstCostItemAmount);
            
            // Act
            var result = await _costService.GetRevisionTotals(cost.LatestCostStageRevision);

            // Asset
            result.totalInLocalCurrency.Should().Be(expectedTotalAmountWithNewCurrencyRate);
            result.total.Should().Be(firstCostItemAmount);
        }

        private ExchangeRate SetupNewExchangeRate(string currencyCode, decimal newRate, DateTime? currencyEffectiveFrom = null)
        {
            var newExchangeRate = new ExchangeRate();
            var effectiveFrom = currencyEffectiveFrom.HasValue
                            ? currencyEffectiveFrom.Value
                            : DateTime.Now;
            switch (currencyCode)
            {
                case "GBP":
                    newExchangeRate = new ExchangeRate
                    {
                        FromCurrency = gbpId,
                        ToCurrency = usdId,
                        Rate = newRate,
                        RateName = "British Pound",
                        EffectiveFrom = effectiveFrom
                    };
                    break;
                case "EUR":
                    newExchangeRate = new ExchangeRate
                    {
                        FromCurrency = eurId,
                        ToCurrency = usdId,
                        Rate = newRate,
                        RateName = "Euro",
                        EffectiveFrom = effectiveFrom
                    };
                    break;
                case "USD":
                    newExchangeRate = new ExchangeRate
                    {
                        FromCurrency = usdId,
                        ToCurrency = usdId,
                        Rate = newRate,
                        RateName = "US Dollar",
                        EffectiveFrom = effectiveFrom
                    };
                    break;
            }
            _efContext.ExchangeRate.AddRange(newExchangeRate);
            _efContext.SaveChanges();
            return newExchangeRate;
        }


        private Cost AddCostLineItems(ExchangeRate oldCostLineExchangeRate, DateTime? exchangeRateDate, decimal costLineItem)
        {
            var latestCostStageRevision = new CostStageRevision
            {
                CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            LocalCurrencyId = oldCostLineExchangeRate.FromCurrency,
                            ValueInLocalCurrency = costLineItem,
                            ValueInDefaultCurrency= (costLineItem * oldCostLineExchangeRate.Rate),
                            Name = "offlineEdits"
                        }
                    },
                Name = "FirstPresentation",
                Modified = DateTime.UtcNow,
                Submitted = exchangeRateDate
            };

            var cost = new Cost
            {
                LatestCostStageRevision = latestCostStageRevision,
                CostStages = new List<CostStage>
                {
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
                CostNumber = "NUMBER011",
                ExchangeRateDate = exchangeRateDate,
                PaymentCurrencyId = usdId,
            };
            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            return cost;
        }

        #endregion
    }
}

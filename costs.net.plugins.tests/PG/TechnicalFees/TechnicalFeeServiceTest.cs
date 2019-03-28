namespace costs.net.plugins.tests.PG.TechnicalFees
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using core.Builders;
    using core.Services.Costs;
    using core.Services.Currencies;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services;

    public class TechnicalFeeServiceTest
    {
        protected ITechnicalFeeService _technicalFeeService;
        protected Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        protected Mock<ICurrencyService> _currencyServiceMock;
        protected Mock<EFContext> _efContext;
        protected Mock<ICostExchangeRateService> _costExchangeRateServiceMock;
        private CostStageRevision _costStageLatestRevision;
        private CostStageRevision _costStagePreviousRevision;
        private List<CostLineItem> _costLineItems;
        private Guid _latestRevisionId = Guid.NewGuid();
        private Guid _costId = Guid.NewGuid();
        private Guid _costStageId = Guid.NewGuid();

        [SetUp]
        public void Init()
        {
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _efContext = new Mock<EFContext>();
            _costExchangeRateServiceMock = new Mock<ICostExchangeRateService>();

            _technicalFeeService = new TechnicalFeeService(_efContext.Object,
                _costStageRevisionServiceMock.Object,
                _currencyServiceMock.Object,
                _costExchangeRateServiceMock.Object);
        }

        private void SetUpCost(bool addCCApprover,
            bool addCurrentTechFeeLineItem,
            bool addNonZeroFormerTechFeeLineItem,
            TechnicalFee applicableFee,
            string localCurrencyCode,
            CostType costType,
            string contentType,
            string productionType,
            string budgetRegion,
            decimal technicalFeeLineItemValue =0)
        {
            var cost = new Cost
            {
                Id = _costId,
                LatestCostStageRevisionId = _latestRevisionId,
                CostType = costType
            };

            var ccApproverUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.CostConsultant,
                            Value = Constants.BusinessRole.CostConsultant
                        }
                    }
                }
            };

            _costStageLatestRevision = new CostStageRevision
            {
                Id = _latestRevisionId,
                Approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ValidBusinessRoles = new[] { Constants.BusinessRole.CostConsultant, "Some other guy" },
                        ApprovalMembers = addCCApprover ? new List<ApprovalMember>() { new ApprovalMember() { CostUser = ccApproverUser } } : new List<ApprovalMember>()
                    }
                }
            };

            _costStagePreviousRevision = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                Approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.IPM,
                        ValidBusinessRoles = new[] { Constants.BusinessRole.CostConsultant, "Some other guy" },
                        ApprovalMembers = addCCApprover ? new List<ApprovalMember>() { new ApprovalMember() { CostUser = ccApproverUser } } : new List<ApprovalMember>()
                    }
                }
            };

            var costStageRevisions = new List<CostStageRevision> { _costStageLatestRevision };

            var feeCurrency = new Currency
            {
                Id = Guid.NewGuid(),
                DefaultCurrency = applicableFee.CurrencyCode == "USD",
                Code = applicableFee.CurrencyCode
            };
            var feeCurrencyExchangeRate = new ExchangeRate
            {
                Rate = 2m
            };

            var localCurrency = new Currency
            {
                Id = Guid.NewGuid(),
                DefaultCurrency = localCurrencyCode == "USD",
                Code = localCurrencyCode
            };
            var localCurrencyExchangeRate = new ExchangeRate
            {
                Rate = 10m
            };

            _costLineItems = new List<CostLineItem>();
            if (addCurrentTechFeeLineItem)
            {
                _costLineItems.Add(new CostLineItem
                {
                    Name = Constants.CostSection.TechnicalFee,
                    CostStageRevisionId = _costStageLatestRevision.Id,
                    LocalCurrencyId = localCurrency.Id,
                    ValueInDefaultCurrency =technicalFeeLineItemValue,
                    ValueInLocalCurrency =technicalFeeLineItemValue
                });
            }

            if (addNonZeroFormerTechFeeLineItem)
            {
                _costLineItems.Add(new CostLineItem
                {
                    Name = Constants.CostSection.TechnicalFee,
                    CostStageRevisionId = _costStagePreviousRevision.Id,
                    LocalCurrencyId = localCurrency.Id,
                    ValueInLocalCurrency = 1000m,
                    ValueInDefaultCurrency = 1000m
                });
            }

            var stageDetails = new PgStageDetailsForm
            {
                ContentType = new DictionaryValue { Key = contentType },
                ProductionType = new DictionaryValue { Key = productionType },
                BudgetRegion = new AbstractTypeValue { Key = budgetRegion }
            };


            var techFees = new List<TechnicalFee> { applicableFee };

            _efContext.MockAsyncQueryable(costStageRevisions.AsQueryable(), x => x.CostStageRevision);
            _costStageRevisionServiceMock.Setup(x => x.GetStageDetails<PgStageDetailsForm>(_latestRevisionId)).ReturnsAsync(stageDetails);
            _costStageRevisionServiceMock.Setup(x => x.GetPreviousRevision(_costStageId)).ReturnsAsync(_costStagePreviousRevision);

            _efContext.MockAsyncQueryable(_costLineItems.AsQueryable(), c => c.CostLineItem);
            _efContext.MockAsyncQueryable(techFees.AsQueryable(), c => c.TechnicalFee);

            _currencyServiceMock.Setup(x => x.GetCurrency(It.IsAny<string>())).ReturnsAsync(feeCurrency);
            _currencyServiceMock.Setup(x => x.GetCurrency(It.IsAny<Guid>())).ReturnsAsync(localCurrency);

            _costExchangeRateServiceMock.Setup(x => x.GetExchangeRateByCurrency(cost.Id, feeCurrency.Id)).ReturnsAsync(feeCurrencyExchangeRate);
            _costExchangeRateServiceMock.Setup(x => x.GetExchangeRateByCurrency(cost.Id, localCurrency.Id)).ReturnsAsync(localCurrencyExchangeRate);
            _efContext.MockAsyncQueryable(new[] { cost }.AsQueryable(), c => c.Cost);

            _efContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        }

        public TechnicalFee GetFee(CostType costType, string contentType, string productionType, string budgetRegion, decimal fee, string currencyCode)
        {
            return new TechnicalFee
            {
                CostType = costType.ToString(),
                ConsultantRate = fee,
                ContentType = contentType,
                CurrencyCode = currencyCode,
                ProductionType = productionType,
                RegionName = budgetRegion
            };
        }

        [Test]
        public void SelectedCostConsultant_ShouldUpdateTechFee()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(true, true, false, fee, "USD", CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _efContext.Verify(x => x.Update(It.Is<CostLineItem>(arg => arg.ValueInLocalCurrency == 200 && arg.ValueInDefaultCurrency == 200)));
        }

        [Test]
        public void RemovingCostConsultant_ShouldUpdateTechnicalFee()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(false, true, true, fee, "USD", CostType.Production, Constants.ContentType.Video.ToString(), Constants.ProductionType.FullProduction.ToString(), Constants.BudgetRegion.China,100);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _costStageLatestRevision.Id);
            _efContext.Verify(x => x.Update(It.Is<CostLineItem>(arg => arg.ValueInDefaultCurrency == 0 && arg.ValueInLocalCurrency == 0 && arg.Name == Constants.CostSection.TechnicalFee)));
        }

        [Test]
        public void NotSelectedCostConsultant_ShouldNotUpdateTechFee()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(false, true, false, fee, "USD", CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _efContext.Verify(x => x.Update(It.IsAny<CostLineItem>()), Times.Never);
        }

        [Test]
        public void SelectedCostConsultant_NoTechFeeLineItem_ShouldNotUpdateTechFee()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(true, false, false, fee, "USD", CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _efContext.Verify(x => x.Update(It.IsAny<CostLineItem>()), Times.Never);
        }

        [Test]
        public void SelectedCostConsultant_NoTechFeeFound_ShouldNotUpdateTechFee()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(true, true, false, fee, "USD", CostType.Production, Constants.ContentType.Audio, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _efContext.Verify(x => x.Update(It.IsAny<CostLineItem>()), Times.Never);
        }

        [Test]
        public void SelectedCostConsultant_ShouldUpdateTechFee_WithCorrectExchangeRate()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(true, true, true, fee, "EUR", CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _efContext.Verify(x => x.Update(It.Is<CostLineItem>(arg => arg.ValueInLocalCurrency == 20 && arg.ValueInDefaultCurrency == 200)));
        }

        [Test]
        public void SelectedCostConsultant_PreviousFeeExists_ShouldNotUpdateValue()
        {
            var fee = GetFee(CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China, 100m, "GBP");
            SetUpCost(false, true, true, fee, "EUR", CostType.Production, Constants.ContentType.Video, Constants.ProductionType.FullProduction, Constants.BudgetRegion.China);
            _technicalFeeService.UpdateTechnicalFeeLineItem(_costId, _latestRevisionId);
            _efContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _efContext.Verify(x => x.Update(It.IsAny<CostLineItem>()), Times.Never);
        }
    }
}

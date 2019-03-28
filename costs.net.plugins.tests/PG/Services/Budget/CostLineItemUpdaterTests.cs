namespace costs.net.plugins.tests.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.CostTemplate;
    using core.Models.Excel;
    using core.Models.User;
    using core.Services;
    using core.Services.Costs;
    using core.Services.CostTemplate;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services.Budget;

    [TestFixture]
    public class CostLineItemUpdaterTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();
        private readonly Mock<ICostStageRevisionService> _revisionServiceMock = new Mock<ICostStageRevisionService>();
        private readonly Mock<ICostTemplateVersionService> _templateServiceMock = new Mock<ICostTemplateVersionService>();
        private readonly Mock<ICostExchangeRateService> _costExchangeRateServiceMock = new Mock<ICostExchangeRateService>();

        private readonly Guid _costId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _costStageRevisionId = Guid.NewGuid();
        private readonly Guid _costTemplateVersionId = Guid.NewGuid();
        private readonly Guid _gbpCurrencyId = Guid.NewGuid();
        private readonly Guid _usdCurrencyId = Guid.NewGuid();

        private List<CostStageRevision> _costStageRevisions;

        private UserIdentity _userIdentity;

        private ProductionDetailsFormDefinitionModel _form;

        private const string ItemName = "taxIfApplicable";
        private const string SectionName = "agencyCosts";
        private const string FormName = "testForm";
        private const string FormOverrideName = "audioForm";

        private CostLineItemUpdater _target;

        [SetUp]
        public void Setup()
        {
            _target = new CostLineItemUpdater(_efContextMock.Object, 
                _revisionServiceMock.Object, 
                _templateServiceMock.Object, 
                new CostSectionFinder(),
                _costExchangeRateServiceMock.Object);
            var cost = new Cost();
            var costStageRevision = new CostStageRevision() {
                CostLineItems = new List<CostLineItem> {
                    new CostLineItem {

                    }
                }
            };
            var costTemplateVersion = new CostTemplateVersion();
            var costTemplate = new CostTemplate();
            var fieldDefinitions = new CustomFormData();
            var usd = new Currency();
            var euro = new Currency();
            var gbp = new Currency();

            cost.Id = _costId;
            costStageRevision.Id = _costStageRevisionId;
            
            cost.LatestCostStageRevision = costStageRevision;
            cost.LatestCostStageRevisionId = _costStageRevisionId;
            cost.CostTemplateVersion = costTemplateVersion;
            cost.CostTemplateVersionId = _costTemplateVersionId;

            costTemplateVersion.CostTemplate = costTemplate;
            costTemplateVersion.Id = _costTemplateVersionId;

            costTemplate.FieldDefinitions = fieldDefinitions;

            euro.Code = "EUR";
            usd.Code = "USD";
            gbp.Code = "GBP";

            euro.Id = Guid.NewGuid();
            usd.Id = _usdCurrencyId;
            gbp.Id = _gbpCurrencyId;

            var costs = new List<Cost> { cost };
            _costStageRevisions = new List<CostStageRevision> { costStageRevision };
            var currencies = new List<Currency> { euro, usd, gbp };

            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), c => c.Cost);
            _efContextMock.MockAsyncQueryable(_costStageRevisions.AsQueryable(), c => c.CostStageRevision);
            _efContextMock.MockAsyncQueryable(currencies.AsQueryable(), c => c.Currency);
            
            var stageDetails = new PgStageDetailsForm
            {
                ContentType = new core.Builders.DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = "Video",
                    Value = "Video"
                },
                CostType = dataAccess.Entity.CostType.Production.ToString(),
                ProductionType = new core.Builders.DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = "Full Production",
                    Value = "Full Production"
                },
                Title = "Cost Title"                
            };
            _revisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(_costStageRevisionId)).ReturnsAsync(stageDetails);

            var costTemplateVersionModel = new CostTemplateVersionModel();
            var productionDetailCollection = new List<ProductionDetailsTemplateModel>();
            var productionDetails = new ProductionDetailsTemplateModel();
            _form = new ProductionDetailsFormDefinitionModel
            {
                Name = FormName,
                Label = "Full production",
                ProductionType = Constants.ProductionType.FullProduction
            };
            var section = new CostLineItemSectionTemplateModel();            
            var item = new CostLineItemSectionTemplateItemModel();

            section.Name = SectionName;
            item.Name = ItemName;

            section.Items = new List<CostLineItemSectionTemplateItemModel>{ item };
            _form.CostLineItemSections = new List<CostLineItemSectionTemplateModel>{ section };
            productionDetails.Forms = new List<ProductionDetailsFormDefinitionModel> { _form };
            productionDetails.Type = "Video";

            productionDetailCollection.Add(productionDetails);
            costTemplateVersionModel.ProductionDetails = productionDetailCollection;
            _templateServiceMock.Setup(ts => ts.GetCostTemplateVersionModel(It.IsAny<Guid>())).ReturnsAsync(costTemplateVersionModel);

            _userIdentity = new UserIdentity
            {
                Id = _userId,
                IpAddress = "127.0.0.1"
            };

            var expectedExchangeRates = new List<ExchangeRate>() {
                new ExchangeRate {
                    FromCurrency = _usdCurrencyId,
                    Rate = 1
                },
            };
            _costExchangeRateServiceMock.Setup(cer => cer.GetExchangeRatesByDefaultCurrency(It.IsAny<Guid>()))
                .ReturnsAsync(expectedExchangeRates);
        }

        [Test]
        public async Task Null_Entries_Returns_No_Updated_CostLineItems()
        {
            //Arrange
            ExcelCellValueLookup entries = null;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            
            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Empty_Entries_Returns_No_Updated_CostLineItems()
        {
            //Arrange
            var entries = new ExcelCellValueLookup();
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_UserId_Returns_No_Updated_CostLineItems()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            UserIdentity userIdentity = null;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_CostId_Returns_No_Updated_CostLineItems()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = Guid.Empty;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_CostStageRevisionId_Returns_No_Updated_CostLineItems()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            var costStageRevisionId = Guid.Empty;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Valid_ExcelValues_Returns_Updated_CostLineItem()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            decimal expectedValueInLocalCurrency = 35000;
            decimal expectedValueInDefaultCurrency = 35000;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(1);
            var cli = result.Result.First();

            cli.Should().NotBeNull();
            cli.ValueInLocalCurrency.Should().Be(expectedValueInLocalCurrency);
            cli.ValueInDefaultCurrency.Should().Be(expectedValueInDefaultCurrency);
        }

        [Test]
        public async Task LockedCostLineItemCurrency_Returns_Failure_WhenEntryHasDifferentCurrency()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;

            var costStageRevision = _costStageRevisions[0];
            costStageRevision.IsLineItemSectionCurrencyLocked = true;
            // GetPrepopulatedLookup returns EUR and USD, not GBP
            var costLineItem = new CostLineItem
            {
                LocalCurrencyId = _gbpCurrencyId
            };
            costLineItem.Name = ItemName;
            costLineItem.CostLineItemSectionTemplate = new CostLineItemSectionTemplate
            {
                Name = SectionName
            };
            costStageRevision.CostLineItems = new List<CostLineItem>
            {
                costLineItem
            };

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task LockedCostLineItemCurrency_Returns_Success_WhenEntryHasSameCurrency()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;

            var costStageRevision = _costStageRevisions[0];
            costStageRevision.IsLineItemSectionCurrencyLocked = true;
            // GetPrepopulatedLookup returns EUR and USD, not GBP
            var costLineItem = new CostLineItem
            {
                LocalCurrencyId = _usdCurrencyId
            };
            costLineItem.Name = ItemName;
            costLineItem.CostLineItemSectionTemplate = new CostLineItemSectionTemplate
            {
                Name = SectionName
            };
            costStageRevision.CostLineItems = new List<CostLineItem>
            {
                costLineItem
            };

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task Form_Section_Item_Overrides_Section_Item()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            decimal expectedValueInLocalCurrency = 13000;
            decimal expectedValueInDefaultCurrency = 13000;
            _form.Name = FormOverrideName;

            //Act
            ServiceResult<List<CostLineItem>> result = await _target.Update(entries, _userIdentity, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(1);
            var cli = result.Result.First();

            cli.Should().NotBeNull();
            cli.ValueInLocalCurrency.Should().Be(expectedValueInLocalCurrency);
            cli.ValueInDefaultCurrency.Should().Be(expectedValueInDefaultCurrency);
        }

        private static ExcelCellValueLookup GetPrepopulatedLookup()
        {
            return new ExcelCellValueLookup
            {
                ["currency.agency"] = new ExcelCellValue
                {
                    Value = "USD"
                },
                ["currency.studio"] = new ExcelCellValue
                {
                    Value = "EUR"
                },
                ["agencyCosts.taxIfApplicable.local"] = new ExcelCellValue
                {
                    Value = "35000"
                },
                ["agencyCosts.taxIfApplicable.default"] = new ExcelCellValue
                {
                    Value = "32000"
                },
                ["audioForm.agencyCosts.taxIfApplicable.local"] = new ExcelCellValue
                {
                    Value = "13000"
                },
                ["audioForm.agencyCosts.taxIfApplicable.default"] = new ExcelCellValue
                {
                    Value = "12000"
                }
            };
        }
    }
}

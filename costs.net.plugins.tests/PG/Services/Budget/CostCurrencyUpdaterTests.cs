namespace costs.net.plugins.tests.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Excel;
    using core.Services;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services.Budget;

    [TestFixture]
    public class CostCurrencyUpdaterTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();
        private readonly Mock<ICostStageRevisionService> _revisionServiceMock = new Mock<ICostStageRevisionService>();

        private readonly Guid _costId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _costStageRevisionId = Guid.NewGuid();
        private readonly Guid _costTemplateVersionId = Guid.NewGuid();
        private readonly Cost _cost = new Cost();
        private readonly CustomFormData _stageFormData = new CustomFormData();
        private readonly CostStageRevision _costStageRevision = new CostStageRevision();
        private readonly PgStageDetailsForm _stageDetails = new PgStageDetailsForm();
        private readonly PgProductionDetailsForm _productionDetails = new PgProductionDetailsForm();

        private CostCurrencyUpdater _target;

        [SetUp]
        public void Setup()
        {
            _target = new CostCurrencyUpdater(_efContextMock.Object, _revisionServiceMock.Object);
            
            var costTemplateVersion = new CostTemplateVersion();
            var costTemplate = new CostTemplate();
            var fieldDefinitions = new CustomFormData();
            var usd = new Currency();
            var euro = new Currency();

            _cost.Id = _costId;
            _costStageRevision.Id = _costStageRevisionId;
            _costStageRevision.StageDetailsId = Guid.NewGuid();


            _cost.LatestCostStageRevision = _costStageRevision;
            _cost.LatestCostStageRevisionId = _costStageRevisionId;
            _cost.CostTemplateVersion = costTemplateVersion;
            _cost.CostTemplateVersionId = _costTemplateVersionId;

            costTemplateVersion.CostTemplate = costTemplate;
            costTemplateVersion.Id = _costTemplateVersionId;

            costTemplate.FieldDefinitions = fieldDefinitions;

            euro.Code = "EUR";
            usd.Code = "USD";

            euro.Id = Guid.NewGuid();
            usd.Id = Guid.NewGuid();

            var customFormData =
                "{\"costNumber\":\"AC1489594599188\",\"isAIPE\":false,\"projectId\":\"58c968410c885409aca51028\",\"title\":\"Full Prod\",\"description\":\"58c968410c885409aca51028\",\"contentType\":{\"id\":\"17cc6250-099a-11e7-84b7-1bf5c65c8e1a\",\"name\":\"Video\"},\"productionType\":{\"id\":\"17bec46a-099a-11e7-84b2-137a2ddbf0ee\",\"name\":\"Full Production\"},\"agencyProducer\":[\"Agency Producer 1\"],\"initialBudget\":1231231,\"budgetRegionId\":\"0f701f02-099a-11e7-945b-4b7e51cf96b3\",\"budgetRegion\":\"GREATER CHINA AREA\",\"organisation\":\"Other\",\"agencyCurrency\":\"USD\",\"campaign\":\"ad58c968410c885409aca51028\",\"agencyTrackingNumber\":\"58c968410c885409aca51028\"}";
            _stageFormData.Data = customFormData;
            _stageFormData.Id = _costStageRevision.StageDetailsId;

            var costs = new List<Cost> { _cost };
            var costStageRevisions = new List<CostStageRevision> { _costStageRevision };
            var currencies = new List<Currency> { euro, usd };

            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), c => c.Cost);
            _efContextMock.MockAsyncQueryable(costStageRevisions.AsQueryable(), c => c.CostStageRevision);
            _efContextMock.MockAsyncQueryable(currencies.AsQueryable(), c => c.Currency);
            _efContextMock.MockAsyncQueryable(new[] { _stageFormData }.AsQueryable(), d => d.CustomFormData);

            _stageDetails.ContentType = new core.Builders.DictionaryValue
            {
                Id = Guid.NewGuid(),
                Key = "Video",
                Value = "Video"
            };
            _stageDetails.CostType = CostType.Production.ToString();
            _stageDetails.ProductionType = new core.Builders.DictionaryValue
            {
                Id = Guid.NewGuid(),
                Key = "Production",
                Value = "Production"
            };
            _stageDetails.Title = "Cost Title";
        
            _revisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(_costStageRevisionId)).ReturnsAsync(_stageDetails);
            _revisionServiceMock.Setup(csr => csr.GetProductionDetails<PgProductionDetailsForm>(_costStageRevisionId)).ReturnsAsync(_productionDetails);
        }

        [Test]
        public async Task Null_Entries_Returns_False_Success()
        {
            //Arrange
            ExcelCellValueLookup entries = null;
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            
            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Empty_Entries_Returns_False_Success()
        {
            //Arrange
            var entries = new ExcelCellValueLookup();
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_UserId_Returns_False_Success()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var userId = Guid.Empty;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_CostId_Returns_False_Success()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var userId = _userId;
            var costId = Guid.Empty;
            var costStageRevisionId = _costStageRevisionId;

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Invalid_CostStageRevisionId_Returns_False_Success()
        {
            //Arrange
            var entries = GetPrepopulatedLookup();
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = Guid.Empty;

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public async Task Update_AgencyCurrency_From_BudgetForm_For_Aipe_InitialStage_Cost()
        {
            //Arrange
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            const string initialAgencyCurrency = "GBP";
            const string expectedAgencyCurrency = "EUR";

            _stageDetails.AgencyCurrency = initialAgencyCurrency;
            var entries = new ExcelCellValueLookup
            {
                ["currency.agency"] = new ExcelCellValue
                {
                    Value = expectedAgencyCurrency
                }
            };
            var aipeStage = new CostStage
            {
                Key = CostStages.Aipe.ToString()
            };
            _costStageRevision.CostStage = aipeStage;
            _costStageRevision.IsPaymentCurrencyLocked = false;

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            _stageFormData.Data.Should().Contain(expectedAgencyCurrency);
        }

        [Test]
        public async Task Update_AgencyCurrency_From_BudgetForm_For_OE_InitialStage_Cost()
        {
            //Arrange
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            const string initialAgencyCurrency = "GBP";
            const string expectedAgencyCurrency = "EUR";

            _stageDetails.AgencyCurrency = initialAgencyCurrency;
            var entries = new ExcelCellValueLookup
            {
                ["currency.agency"] = new ExcelCellValue
                {
                    Value = expectedAgencyCurrency
                }
            };
            var originalEstimateStage = new CostStage
            {
                Key = CostStages.OriginalEstimate.ToString()
            };
            _costStageRevision.CostStage = originalEstimateStage;
            _costStageRevision.IsPaymentCurrencyLocked = false;
            originalEstimateStage.CostStageRevisions = new List<CostStageRevision>
            {
                _costStageRevision
            };

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _stageFormData.Data.Should().Contain(expectedAgencyCurrency);
        }

        [Test]
        public async Task Do_Not_Update_AgencyCurrency_From_BudgetForm_For_LockedPaymentCurrency()
        {
            //Arrange
            var userId = _userId;
            var costId = _costId;
            var costStageRevisionId = _costStageRevisionId;
            const string initialAgencyCurrency = "GBP";
            const string expectedAgencyCurrency = "EUR";

            _stageDetails.AgencyCurrency = initialAgencyCurrency;
            var entries = new ExcelCellValueLookup
            {
                ["currency.agency"] = new ExcelCellValue
                {
                    Value = expectedAgencyCurrency
                }
            };
            var originalEstimateStage = new CostStage
            {
                Key = CostStages.OriginalEstimate.ToString()
            };

            var originalEstimateRevisionStage = new CostStage
            {
                Key = CostStages.OriginalEstimateRevision.ToString()
            };
            _costStageRevision.CostStage = originalEstimateRevisionStage;
            _costStageRevision.IsPaymentCurrencyLocked = true;

            var firstRevision = new CostStageRevision();
            firstRevision.CostStage = originalEstimateStage;
            firstRevision.IsPaymentCurrencyLocked = false;
            originalEstimateRevisionStage.CostStageRevisions = new List<CostStageRevision>
            {
                firstRevision,
                _costStageRevision
            };

            //Act
            ServiceResult result = await _target.Update(entries, userId, costId, costStageRevisionId);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().HaveCount(1);
            _stageFormData.Data.Should().NotContain(expectedAgencyCurrency);
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
                }
            };
        }
    }
}

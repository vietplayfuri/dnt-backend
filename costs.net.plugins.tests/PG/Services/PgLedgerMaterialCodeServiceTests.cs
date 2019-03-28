namespace costs.net.plugins.tests.PG.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Builders.Response;
    using core.Models.User;
    using core.Services.CustomData;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models;
    using plugins.PG.Services;

    [TestFixture]
    public class PgLedgerMaterialCodeServiceTests
    {
        private const string MultipleOptions = "Multiple";
        private Mock<EFContext> _efContextMock;
        private Mock<ICustomObjectDataService> _customObjectDataServiceMock;
        private PgLedgerMaterialCodeService _sut;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _customObjectDataServiceMock = new Mock<ICustomObjectDataService>();
            var adminUser = new CostUser { Id = Guid.NewGuid(), Email = ApprovalMemberModel.BrandApprovalUserEmail };            
            var users = new List<CostUser> { adminUser };
            _efContextMock.MockAsyncQueryable(users.AsQueryable(), c => c.CostUser);

            SetupDictionaries();

            _sut = new PgLedgerMaterialCodeService(_efContextMock.Object, _customObjectDataServiceMock.Object);
        }

        [Test]
        public async Task UpdateLedgerMaterialCode_whenCostTypeProductionAndOnlyContentTypeMatched_shouldApplyDefaultValues()
        {
            // Arrange
            var costStageRevisionId = Guid.NewGuid();
            var contentTypeId = Guid.NewGuid();
            var costType = CostType.Production;
            var contentType = Constants.ContentType.Video;
            var accountCode = "Test account code";
            var mgCode = "Test mg code";
            var costStageRevision = new CostStageRevision
            {
                Id = costStageRevisionId,
                StageDetails = new CustomFormData
                {
                    Data = JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        ContentType = new DictionaryValue
                        {
                            Id = contentTypeId,
                            Key = contentType,
                            Value = contentType
                        }
                    })
                },
                ExpectedAssets = new List<ExpectedAsset>(),
                CostStage = new CostStage
                {
                    Cost = new Cost { CostType = costType }
                }
            };
            _efContextMock.MockAsyncQueryable(new List<CostStageRevision> { costStageRevision }.AsQueryable(), c => c.CostStageRevision);

            var ledgerMaterialCodes = new List<PgLedgerMaterialCode>
            {
                new PgLedgerMaterialCode
                {
                    CostType = costType.ToString(),
                    ContentTypeId = contentTypeId,
                    GeneralLedgerCode = accountCode,
                    MaterialGroupCode = mgCode
                }
            };
            _efContextMock.MockAsyncQueryable(ledgerMaterialCodes.AsQueryable(), c => c.PgLedgerMaterialCode);

            // Act
            await _sut.UpdateLedgerMaterialCodes(costStageRevisionId);

            // Assert
            _customObjectDataServiceMock.Verify(cod => cod.Save(
                It.Is<Guid>(id => id == costStageRevisionId),
                It.Is<string>(key => key == CustomObjectDataKeys.PgMaterialLedgerCodes),
                It.Is<PgLedgerMaterialCodeModel>(model => model.GlCode == accountCode && model.MgCode == mgCode), 
                It.IsAny<UserIdentity>()
                ), Times.Once);
        }

        private void SetupDictionaries()
        {
            var dictionaryEntries = new List<DictionaryEntry>
            {
                new DictionaryEntry
                {
                    Key = MultipleOptions,
                    Dictionary = new dataAccess.Entity.Dictionary
                    {
                        Name = Constants.DictionaryNames.OvalType
                    }
                },
                new DictionaryEntry
                {
                    Key = MultipleOptions,
                    Dictionary = new dataAccess.Entity.Dictionary
                    {
                        Name = Constants.DictionaryNames.MediaType
                    }
                }
            };
            _efContextMock.MockAsyncQueryable(dictionaryEntries.AsQueryable(), c => c.DictionaryEntry);
        }
    }
}

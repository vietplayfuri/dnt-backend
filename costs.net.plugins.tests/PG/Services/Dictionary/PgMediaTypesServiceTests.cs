namespace costs.net.plugins.tests.PG.Services.Dictionary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services.Dictionary;

    [TestFixture]
    public class PgMediaTypesServiceTests
    {
        private EFContext _efContext;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private PgMediaTypesService _service;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            var dictionary = new Dictionary
            {
                Name = Constants.DictionaryNames.MediaType,
                DictionaryEntries = new[]
                    {
                        Constants.MediaType.Digital,
                        Constants.MediaType.InStore,
                        Constants.MediaType.NA,
                        Constants.MediaType.NotForAir,
                        Constants.MediaType.OutOfHome,
                        Constants.MediaType.PRInfluencer,
                        Constants.MediaType.Radio,
                        Constants.MediaType.StreamingAudio,
                        Constants.MediaType.Tv,
                        Constants.MediaType.Cinema,
                        Constants.MediaType.DirectToCustomer,
                        Constants.MediaType.Multiple
                    }
                    .Select(i => new DictionaryEntry { Key = i })
                    .ToList()
            };
            _efContext.Dictionary.Add(dictionary);
            _efContext.SaveChanges();

            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _service = new PgMediaTypesService(_efContext, _costStageRevisionServiceMock.Object);
        }

        [Test]
        public async Task GetMediaTypes_Types_WhenCostTypeIsTrafficking_ShouldReturnValidListOfMediaTypes()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var pgStageDetailsForm = new PgStageDetailsForm();

            var revision = new CostStageRevision
            {
                Id = revisionId,
                StageDetails = new CustomFormData
                {
                    Data = JsonConvert.SerializeObject(pgStageDetailsForm)
                },
                CostStage = new CostStage
                {
                    Cost = new Cost
                    {
                        CostType = CostType.Trafficking
                    }
                }
            };
            _efContext.CostStageRevision.Add(revision);
            _efContext.SaveChanges();
            _costStageRevisionServiceMock.Setup(s =>
                    s.GetStageDetails<PgStageDetailsForm>(revision))
                .Returns(pgStageDetailsForm);

            // Act
            var result = await _service.GetMediaTypes(revisionId);

            // Assert
            result.Select(i => i.Key).Should().BeEquivalentTo(Constants.MediaType.NA);
        }
    }
}
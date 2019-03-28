namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using core.Models.Costs;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.Dictionary;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using net.tests.common.Stubs.EFContext;

    [TestFixture]
    public class UpdateExpectedAssetShould
    {
        private EFContext _efContext;
        private Mock<IEventService> _eventService;
        private Mock<IActivityLogService> _activityLogService;
        private Mock<IMediaTypesService> _mediaTypeServiceMock;

        private ExpectedAssetService _service;
        private Guid _assetId;
        private ExpectedAsset _asset;
        private SaveExpectedAssetModel _assetSaveModel;
        private Guid _costId;
        private Guid _costStageRevisionId;
        private UserIdentity _userIdentity;

        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _eventService = new Mock<IEventService>();
            _activityLogService = new Mock<IActivityLogService>();
            _mediaTypeServiceMock = new Mock<IMediaTypesService>();
            _service = new ExpectedAssetService(_efContext, 
                _eventService.Object, _activityLogService.Object, _mediaTypeServiceMock.Object);

            _assetId = Guid.NewGuid();
            _costId = Guid.NewGuid();
            _costStageRevisionId = Guid.NewGuid();

            var cost = new Cost
            {
                CostNumber = "Test101",
                Id = _costId
            };
            _asset = GetExistingAsset(_assetId, cost);
            _assetSaveModel = GetAssetSaveModel();
            _mediaTypeServiceMock.Setup(s => s.GetMediaTypesByCostStageRevision(
                It.IsAny<UserIdentity>(),
                It.IsAny<Guid>()
            )).ReturnsAsync(new[]
            {
                new DictionaryEntry
                {
                    Id = _assetSaveModel.MediaTypeId ?? Guid.Empty
                }
            });

            _userIdentity = new UserIdentity
            {
                Id = Guid.NewGuid(),
                IpAddress = "127.0.0.1",
                Email = "test@adstream.com"
            };
            _efContext.ExpectedAsset.Add(_asset);
            _efContext.Cost.Add(cost);

            _efContext.SaveChanges();
        }

        private ExpectedAsset GetExistingAsset(Guid assetId, Cost cost)
        {
            return new ExpectedAsset
            {
                ProjectAdIdId = Guid.NewGuid(),
                AiringRegions = new[] { "UK", "USA" },
                AssetId = null,
                CostStageRevisionId = new Guid("b82dda6e-faa2-11e6-92a6-005056c00001"),
                Created = DateTime.Now,
                CreatedById = new Guid(),
                Definition = string.Empty,
                Duration = null,
                FirstAirDate = DateTime.Now,
                Id = assetId,
                Initiative = "Test",
                MediaTypeId = new Guid(),
                Modified = DateTime.Now,
                OvalTypeId = new Guid(),
                Scrapped = true,
                Title = "Test Asset Title",
                CostStageRevision = new CostStageRevision
                {
                    CostStage = new CostStage
                    {
                        Cost = cost
                    }
                },
                ProjectAdId = new ProjectAdId()
            };
        }

        public SaveExpectedAssetModel GetAssetSaveModel()
        {
            return new SaveExpectedAssetModel
            {
                AdId = "A2ID1910000",
                AiringRegions = new[] { "UK", "USA" },
                AssetId = null,
                Definition = string.Empty,
                Duration = null,
                FirstAirDate = DateTime.Now,
                Initiative = "Updated Test",
                MediaTypeId = new Guid(),
                OvalTypeId = new Guid(),
                Scrapped = true,
                Title = "Test Asset Title"
            };
        }

        [Test]
        public async Task SaveCorrectDurationIfValueOver24H()
        {
            // invalid timespan - should fall back onto int parse resulting in 1.16:00:00
            _assetSaveModel.Duration = "40:00:00";
            var expectedDuration = new TimeSpan(1, 16, 0, 0);
            await _service.UpdateExpectedAsset(_costId, _costStageRevisionId, _userIdentity, _assetId, _assetSaveModel);

            var result = _efContext.ExpectedAsset.Single(ea => ea.Id == _assetId);
            result.Duration.Should().Be(expectedDuration);
        }

        [Test]
        public async Task SaveCorrectDurationIfValueUnder24H()
        {
            // valid timespan - should show up as 12:34:56
            _assetSaveModel.Duration = "12:34:56";
            var expected = new TimeSpan(12, 34, 56);
            await _service.UpdateExpectedAsset(_costId, _costStageRevisionId, _userIdentity, _assetId, _assetSaveModel);

            var result = _efContext.ExpectedAsset.Single(ea => ea.Id == _assetId);
            result.Duration.Should().Be(expected);
        }

        [Test]
        public async Task SaveDurationAsZeroIfValueInvalid()
        {
            // invalid timespan - should result in zero timespan
            _assetSaveModel.Duration = "INVALID";
            var expected = TimeSpan.Zero;
            await _service.UpdateExpectedAsset(_costId, _costStageRevisionId, _userIdentity, _assetId, _assetSaveModel);

            var result = _efContext.ExpectedAsset.Single(ea => ea.Id == _assetId);
            result.Duration.Should().Be(expected);
        }

        [Test]
        public async Task SaveSetCostUserModified()
        {
            var expected = TimeSpan.Zero;
            await _service.UpdateExpectedAsset(_costId, _costStageRevisionId, _userIdentity, _assetId, _assetSaveModel);

            var result = _efContext.ExpectedAsset.Single(ea => ea.Id == _assetId);
            result.Duration.Should().Be(expected);
        }
    }
}
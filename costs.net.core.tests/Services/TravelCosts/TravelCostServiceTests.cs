namespace costs.net.core.tests.Services.TravelCosts
{
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Mapping;
    using dataAccess;
    using core.Services.TravelCosts;
    using core.Models.Costs;
    using core.Models.User;
    using core.Services.ActivityLog;
    using dataAccess.Entity;
    using net.tests.common.Stubs.EFContext;

    [TestFixture]
    public class TravelCostServiceTest
    {
        private UserIdentity _userIdentity;
        private Mock<EFContext> _efContextMock;
        private TravelCostService _service;
        private UpdateTravelCurrencyModel _model;
        private IEnumerable<TravelCost> _travelCost;
        private IMapper _mapper;
        private Guid _costId;
        private Guid _revisionId;

        [SetUp]
        public void Setup()
        {
            _efContextMock = new Mock<EFContext>();
            var activityLogServiceMock = new Mock<IActivityLogService>();

            _model = GetUpdateCurrencyModel();
            _travelCost = GetTravelCost();

            _costId = new Guid("4cf1547a-71c0-41b8-a044-c0d247a60e39");
            _revisionId = Guid.NewGuid();

            _userIdentity = new UserIdentity
            {
                Id = Guid.NewGuid(),
                IpAddress = "127.0.0.1"
            };

            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<TravelProfile>();
            }));

            _efContextMock.MockAsyncQueryable(_travelCost.AsQueryable(), d => d.TravelCost);

            _service = new TravelCostService(_efContextMock.Object, activityLogServiceMock.Object, _mapper);
        }
        
        private static UpdateTravelCurrencyModel GetUpdateCurrencyModel()
        {
            return new UpdateTravelCurrencyModel
            {                
                SourceRate = 1,
                TargetRate = Convert.ToDecimal(1.2183235883712769),             
            };
        }

        private static IEnumerable<TravelCost> GetTravelCost()
        {
            var result = new List<TravelCost>
            {
                new TravelCost {
                    Id = new Guid("1530ddc8-3636-11e7-94cb-0250f278f600"),
                    CostStageRevisionId = new Guid("e273c6be-46c8-46f2-be49-40dcbd29740c"),
                    Name = "Gregory Isaac",
                    Role = "Producer",
                    ShootDays = 4,
                    RegionId = new Guid("449c1b06-3580-11e7-9efb-0250f278f600"),
                    CountryId = new Guid("457df1a2-3580-11e7-9f30-0250f278f600"),
                    ShootCity = "London",
                    PerDiem = 300,
                    TravelType = "air",
                    TravelTypeCost = 1500,
                    OtherContentType = false
                }
            };

            return result;
        }

        private IEnumerable<CostStage> GetCostStage()
        {
            return new List<CostStage>
            {
                new CostStage
                {
                    CostId = new Guid("4cf1547a-71c0-41b8-a044-c0d247a60e39"),
                    Id = new Guid("82261248-400f-4268-ac0c-6eeb7b01efe2"),
                    StageOrder = 1,
                    Key = "OriginalEstimate",
                    Name = "Original Estimate",
                    CreatedById = new Guid("4ae04884-3580-11e7-b604-0250f278f600")
                }
            };
        }

        private static IEnumerable<CostStageRevision> GetCostStageRevision()
        {
            return new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    Id = new Guid("e273c6be-46c8-46f2-be49-40dcbd29740c"),
                    CostStageId = new Guid("82261248-400f-4268-ac0c-6eeb7b01efe2"),
                    Name = "OriginalEstimate",
                    StageDetailsId = new Guid("8b954b57-fc51-464b-a95f-6bef1bded476"),
                    ProductDetailsId = new Guid("d6f215bb-61ab-4384-b65b-e165227e4ba7"),
                    Status = 0,
                    CreatedById = new Guid("4ae04884-3580-11e7-b604-0250f278f600")
                }
            };
        }

        [Test]
        public async Task Travel_Cost_Update_Exchange_Rate_To_Foreign_Currency_Async()
        {
            //Convert from USD to British Pound

            _model.SourceRate = 1;
            _model.TargetRate = Convert.ToDecimal(1.22);

            await _service.UpdateExchangeRateAsync(_costId, _revisionId, _model, _userIdentity);

            _efContextMock.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Travel_Cost_Update_Exchange_Rate_To_USD_Async()
        {
            //Convert from British Pound to USD
 
            _model.SourceRate = Convert.ToDecimal(1.22);
            _model.TargetRate = 1;

            await _service.UpdateExchangeRateAsync(_costId, _revisionId, _model, _userIdentity);

            _efContextMock.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Travel_Cost_Update_Exchange_Rate_FROM_GB_TO_EUR_Async()
        {
            //Convert from British Pound to Euros

            _model.SourceRate = 1;
            _model.TargetRate = Convert.ToDecimal(1.22);

            await _service.UpdateExchangeRateAsync(_costId, _revisionId, _model, _userIdentity);

            _efContextMock.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Travel_Cost_Update_Exchange_Rate_To_Foreign_Currency_when_CostStageRevisionId_is_zero_Async()
        {
            //Convert from USD to British Pound. This scenario covers the situation when the front end is not refreshed the user 
            //change the default currency straight after adding a travel cost item.

            var costStageData = GetCostStage();
            var costStageRevisionData = GetCostStageRevision();
            var cost = new Cost { LatestCostStageRevisionId = Guid.NewGuid() };
            
            _model.SourceRate = 1;
            _model.TargetRate = Convert.ToDecimal(1.22);          

            _efContextMock.MockAsyncQueryable(new[] { cost }.AsQueryable(), d => d.Cost)
                .Setup(c => c.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync(cost);

            _efContextMock.MockAsyncQueryable(costStageData.AsQueryable(), d => d.CostStage);
            _efContextMock.MockAsyncQueryable(costStageRevisionData.AsQueryable(), d => d.CostStageRevision);

            await _service.UpdateExchangeRateAsync(_costId, _revisionId, _model, _userIdentity);

            _efContextMock.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        }
    }

    
}

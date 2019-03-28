namespace costs.net.integration.tests.Plugins.PG
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using core.Builders;
    using core.Services.Costs;
    using core.Services.Currencies;
    using dataAccess.Entity;
    using DotLiquid.Util;
    using Moq;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Form;
    using plugins.PG.Services;

    public class TechnicalFeeIntegrationTestBase : BaseCostIntegrationTest
    {
        protected ITechnicalFeeService _technicalFeeService;
        protected string _contentType;
        protected Cost _cost;
        protected Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        protected Mock<ICurrencyService> _currencyServiceMock;        
        protected Mock<ICostExchangeRateService> _costExchangeRateServiceMock;

        protected PgProductionDetailsForm _productionDetails;
        protected string _productionType;
        protected CostStageRevision _revision;
        
        protected PgStageDetailsForm _stageDetails;

        [SetUp]
        public void Setup()
        {
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _currencyServiceMock = new Mock<ICurrencyService>();            
            _costExchangeRateServiceMock = new Mock<ICostExchangeRateService>();

            _technicalFeeService = new TechnicalFeeService(EFContext,
                _costStageRevisionServiceMock.Object,               
                _currencyServiceMock.Object,
                _costExchangeRateServiceMock.Object);
        }

        protected void InitData(
            string budgetRegion = Constants.BudgetRegion.AsiaPacific,
            string contentType = Constants.ContentType.Photography,
            string productionType = Constants.ProductionType.FullProduction,
            CostType costType = CostType.Production
        )
        {
            CreateTemplateIfNotCreated(User).Wait();

            var costTemplateId = costType == CostType.Production 
                ? CostTemplate.Id 
                : costType == CostType.Buyout 
                    ? UsageCostTemplate.Id
                    : TrafficCostTemplate.Id;

            var costModel = new CreateCostModelBuilder()
                .WithBudgetRegion(budgetRegion)
                .WithContentType(contentType)
                .WithProductionType(productionType)
                .WithTemplateId(costTemplateId)
                .Build();

            var createCostResponse = CreateCost(User, costModel).Result;
            _cost = Deserialize<Cost>(createCostResponse.Body);
            _revision = EFContext.CostStageRevision.First(csr => csr.Id == _cost.LatestCostStageRevisionId.Value);
            var costStageRevisionId = _revision.Id;

            _contentType = contentType;
            _productionType = productionType;
            _stageDetails = new PgStageDetailsForm
            {
                ContentType = new DictionaryValue { Key = _contentType },
                ProductionType = new DictionaryValue { Key = _productionType },
                BudgetRegion = new AbstractTypeValue { Key = budgetRegion }
            };
            _productionDetails = new PgProductionDetailsForm();

            _costStageRevisionServiceMock.Setup(csr => csr.GetRevisionById(costStageRevisionId)).ReturnsAsync(_revision);
            _costStageRevisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(costStageRevisionId)).ReturnsAsync(_stageDetails);
            _costStageRevisionServiceMock.Setup(csr => csr.GetProductionDetails<PgProductionDetailsForm>(costStageRevisionId)).ReturnsAsync(_productionDetails);
        }


        private async Task CreateTemplateIfNotCreated(CostUser owner)
        {
            if (CostTemplate == null)
            {

                CostTemplate = await CreateTemplate(owner);
            }
            if (UsageCostTemplate == null)
            {

                UsageCostTemplate = await CreateUsageTemplate(owner);
            }
            if (TrafficCostTemplate == null)
            {

                TrafficCostTemplate = await CreateTrafficTemplate(owner);
            }
        }
    }
}

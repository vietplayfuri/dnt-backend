namespace costs.net.integration.tests.Plugins.PG
{
    using core.Builders;
    using core.Builders.Request;
    using core.Models.Costs;
    using core.Services.Agency;
    using core.Services.Costs;
    using core.Services.Project;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using plugins.PG.Builders.Cost;
    using plugins.PG.Builders.Workflow;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services;
    using core.Services.CostTemplate;
    using core.Services.Currencies;
    using dataAccess.Views;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using plugins.PG.Builders.Payments;
    using plugins.PG.Form;
    using plugins.PG.Services.Costs;

    public class ApprovalRuleIntegrationTestBase : BaseIntegrationTest
    {
        private Mock<IPgStageBuilder> _pgStageBuilderMock;
        private Mock<ICostStageRevisionService> _costStageRevisionService;
        private Mock<IAgencyService> _agencyService;
        private Mock<IProjectService> _projectService;
        private Mock<EFContext> _efContext;
        protected ICostBuilder _pgCostBuilder;
        private Mock<ICostNumberGeneratorService> _costNumberGeneratorServiceMock;
        private Mock<IPgLedgerMaterialCodeService> _pgLedgerMaterialCodeServiceMock;
        private Mock<ICostLineItemService> _costLineItemServiceMock;
        private Mock<ICostTemplateVersionService> _costTemplateServiceMock;
        private Mock<IPermissionService> _permissionServiceMock;
        private Mock<IPgCostService> _pgCostServiceMock;
        private Mock<IPgPaymentService> _pgPaymentServiceMock;
        private Mock<IExchangeRateService> _exchangeRateServiceMock;
        private IPgCostSectionTotalsBuilder _pgTotalsBuilder;

        private const string _costNumber = "PG001";

        [SetUp]
        public void Init()
        {
            
            _pgStageBuilderMock = new Mock<IPgStageBuilder>();
            _projectService = new Mock<IProjectService>();
            var currencyService = new Mock<IPgCurrencyService>();
            _agencyService = new Mock<IAgencyService>();
            var ruleService = GetService<IRuleService>();
            
            _costStageRevisionService = new Mock<ICostStageRevisionService>();
            _pgLedgerMaterialCodeServiceMock = new Mock<IPgLedgerMaterialCodeService>();
            _efContext = new Mock<EFContext>();
            _costNumberGeneratorServiceMock = new Mock<ICostNumberGeneratorService>();
            _costNumberGeneratorServiceMock.Setup(x => x.Generate(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(_costNumber));
            _costLineItemServiceMock = new Mock<ICostLineItemService>();
            _costTemplateServiceMock = new Mock<ICostTemplateVersionService>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _pgCostServiceMock = new Mock<IPgCostService>();
            _pgTotalsBuilder = new PgCostSectionTotalsBuilder();
            _pgPaymentServiceMock = new Mock<IPgPaymentService>();
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _efContext.MockAsyncQueryable(new[]
            {
                new Cost
                {
                    LatestCostStageRevision = new CostStageRevision
                    {
                        CostStage = new CostStage
                        {
                            Key = nameof(CostStages.OriginalEstimate)
                        }
                    }
                }
            }.AsQueryable(), c => c.Cost);

            _pgCostBuilder = new PgCostBuilder(
                _pgStageBuilderMock.Object, 
                ruleService, 
                _costStageRevisionService.Object, 
                _agencyService.Object, 
                _projectService.Object,
                _efContext.Object,
                _costNumberGeneratorServiceMock.Object,
                currencyService.Object,
                _pgLedgerMaterialCodeServiceMock.Object,
                _costLineItemServiceMock.Object,
                _costTemplateServiceMock.Object,
                _permissionServiceMock.Object,
                _pgCostServiceMock.Object,
                _pgTotalsBuilder,
                _pgPaymentServiceMock.Object,
                _exchangeRateServiceMock.Object
                );
        }

        protected void SetupCycloneAgencyMock()
        {
            _agencyService.Setup(x => x.GetAgencyByCostId(It.IsAny<Guid>())).ReturnsAsync(new Agency { Labels = new[] { "Cyclone" } });
        }

        protected IStageDetails BuildStageDetails(Guid revisionId, string contentType, decimal budget, string region, 
                CostType costType, string productionType = null, bool? isUsage = null, CostStages costStage = CostStages.OriginalEstimate)
        {
            _costStageRevisionService.Setup(x => x.GetCostLineItems(It.IsAny<Guid>()))
                .ReturnsAsync(new List<CostLineItemView>() { new CostLineItemView() { ValueInDefaultCurrency = budget } });
            var t = new List<CostStageRevision>()
            {
                new CostStageRevision()
                {
                    Id = revisionId,
                    CostStage = new CostStage() { Key = costStage.ToString() }
                }
            };

            _efContext.MockAsyncQueryable(t.AsQueryable(), x => x.CostStageRevision);

            var result = new StageDetails()
            {
                Data = new Dictionary<string, dynamic>()
            };

            result.Data.Add("initialBudget", budget);
            result.Data.Add("budgetRegion", new AbstractTypeValue {Key = region});
            if (costType == CostType.Production)
            {
                result.Data.Add("contentType", JObject.Parse("{\"id\":\"b4a1bb22-90ab-4e37-82bd-494e512827da\",\"value\":\"" + contentType + "\",\"key\":\"" + contentType + "\"}"));
                result.Data.Add("productionType",
                    JObject.Parse("{\"id\":\"b4a1bb22-90ab-4e37-82bd-494e512827dd\",\"value\":\"" + productionType + "\",\"key\":\"" + productionType + "\"}"));
            }

            result.Data.Add("costType", costType);
            if (isUsage.HasValue)
            {
                result.Data.Add("isUsage", isUsage.Value);
                result.Data.Add("usageBuyoutType", new DictionaryValue { Key = contentType });
            }
            return result;
        }

        protected void SetPreviousRevision(CostStages costStage)
        {
            var previous = new CostStageRevision()
            {
                Id = Guid.NewGuid(),
                Name = costStage.ToString(),
                CostStage = new CostStage() { Key = costStage.ToString() }
            };
            _costStageRevisionService.Setup(s => s.GetPreviousRevision(It.IsAny<Guid>())).ReturnsAsync(previous);
        }
    }
}

namespace costs.net.integration.tests.Plugins.PG.SupportingDocumentRules
{
    using core.Builders;
    using core.Builders.Request;
    using core.Builders.Rules;
    using core.Models;
    using core.Models.Costs;
    using core.Services.Agency;
    using core.Services.Costs;
    using core.Services.Project;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using plugins.PG.Builders.Cost;
    using plugins.PG.Builders.Workflow;
    using System;
    using System.Collections.Generic;
    using plugins.PG.Services;
    using System.Threading.Tasks;
    using core.Services;
    using core.Services.CostTemplate;
    using core.Services.Currencies;
    using plugins.PG.Builders.Payments;
    using plugins.PG.Form;
    using plugins.PG.Services.Costs;

    public class SupportingDocumentsTestBase : BaseIntegrationTest
    {
        internal PgCostBuilder CostBuilder;
        private readonly Mock<ICostNumberGeneratorService> _costNumberGeneratorServiceMock = new Mock<ICostNumberGeneratorService>();
        private Mock<IPgLedgerMaterialCodeService> _pgLedgerMaterialCodeServiceMock;
        private Mock<ICostLineItemService> _costLineItemServiceMock;
        private Mock<ICostTemplateVersionService> _costTemplateServiceMock;
        private Mock<IPermissionService> _permissionServiceMock;
        private Mock<IPgPaymentService> _pgPaymentServiceMock;
        private Mock<IExchangeRateService> _exchangeRateServiceMock;
        private IPgCostSectionTotalsBuilder _pgTotalsBuilder;
        private const string CostNumber = "PG001";
        protected const string PreviousStageId = "b4a1bb22-90ab-4e37-82bd-494e512827da";

        [SetUp]
        public void Setup()
        {
            var mockIPgStageBuilder = new Mock<IPgStageBuilder>();
            //IRuleService ruleService,
            var mockICostStageRevisionService = new Mock<ICostStageRevisionService>();
            mockICostStageRevisionService.Setup(a => a.GetPreviousRevision(new Guid(PreviousStageId)))
                .ReturnsAsync(new CostStageRevision() 
                {
                    Name = "OriginalEstimate"
                });
            var mockIAgencyService = new Mock<IAgencyService>();
            var mockIProjectService = new Mock<IProjectService>();
            var mockRuleBuilder = new Mock<IVendorRuleBuilder>();
            var mockRuleService = new Mock<IPluginRuleService>();
            var pgCostServiceMock = new Mock<IPgCostService>();
            var efContext = GetService<EFContext>();
            var currencyService = new Mock<IPgCurrencyService>();
            _costLineItemServiceMock = new Mock<ICostLineItemService>();
            _costTemplateServiceMock = new Mock<ICostTemplateVersionService>();
            _permissionServiceMock = new Mock<IPermissionService>();

            var ruleEngine = new RuleEngine();
            var ruleService = new RuleService(
                new List<Lazy<IVendorRuleBuilder, PluginMetadata>>
                {
                    new Lazy<IVendorRuleBuilder, PluginMetadata>(() => mockRuleBuilder.Object, new PluginMetadata { BuType = BuType.Pg })
                },
                new List<Lazy<IPluginRuleService, PluginMetadata>>
                {
                    new Lazy<IPluginRuleService, PluginMetadata>(() => mockRuleService.Object, new PluginMetadata { BuType = BuType.Pg })
                },
                ruleEngine,
                efContext
            );
            _costNumberGeneratorServiceMock.Setup(x => x.Generate(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>() ,It.IsAny<int>())).Returns(Task.FromResult(CostNumber));
            _pgLedgerMaterialCodeServiceMock = new Mock<IPgLedgerMaterialCodeService>();
            _pgPaymentServiceMock = new Mock<IPgPaymentService>();
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _pgTotalsBuilder = new PgCostSectionTotalsBuilder();

            CostBuilder = new PgCostBuilder(mockIPgStageBuilder.Object,
                ruleService,
                mockICostStageRevisionService.Object,
                mockIAgencyService.Object,
                mockIProjectService.Object,
                efContext,
                _costNumberGeneratorServiceMock.Object,
                currencyService.Object,
                _pgLedgerMaterialCodeServiceMock.Object,
                _costLineItemServiceMock.Object,
                _costTemplateServiceMock.Object,
                _permissionServiceMock.Object,
                pgCostServiceMock.Object,
                _pgTotalsBuilder,
                _pgPaymentServiceMock.Object,
                _exchangeRateServiceMock.Object
                );
        }

        protected IStageDetails BuildStageDetails(string contentType, string region, CostType costType, string productionType, bool? isUsage = null)
        {
            var result = new StageDetails
            {
                Data = new Dictionary<string, dynamic>()
            };

            result.Data.Add("budgetRegion", new AbstractTypeValue { Key = region });
            result.Data.Add("contentType", JObject.Parse("{\"id\":\"b4a1bb22-90ab-4e37-82bd-494e512827da\",\"value\":\"" + contentType + "\",\"key\":\"" + contentType + "\"}"));
            if (costType == CostType.Production)
            {               
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
    }
}

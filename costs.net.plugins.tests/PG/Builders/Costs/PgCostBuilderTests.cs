namespace costs.net.plugins.tests.PG.Builders.Costs
{
    using core.Services.Agency;
    using core.Services.Costs;
    using core.Services.Project;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Builders.Cost;
    using plugins.PG.Builders.Workflow;
    using plugins.PG.Models.Stage;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Services;
    using core.Services.CostTemplate;
    using core.Services.Currencies;
    using FluentAssertions;
    using Newtonsoft.Json;
    using plugins.PG.Builders.Payments;
    using plugins.PG.Form;
    using plugins.PG.Services;
    using plugins.PG.Services.Costs;

    [TestFixture]
    public class PgCostBuilderTests
    {
        protected PgCostBuilder CostBuilder;
        protected Mock<IPgStageBuilder> PgStageBuilderMock;
        protected Mock<IRuleService> RuleServiceMock;
        protected Mock<ICostStageRevisionService> CostStageRevisionServiceMock;
        protected Mock<IAgencyService> AgencyServiceMock;
        protected Mock<IProjectService> ProjectServiceMock;
        protected Mock<IPgLedgerMaterialCodeService> PgLedgerMaterialCodeServiceMock;
        protected Mock<ICostLineItemService> CostLineItemServiceMock;
        protected Mock<ICostNumberGeneratorService> CostNumberGeneratorServiceMock;
        protected Mock<ICostTemplateVersionService> CostTemplateVersionServiceMock;
        protected Mock<IPermissionService> PermissionServiceMock;
        protected EFContext EFContext;
        private Mock<IPgCostService> _pgCostServiceMock;
        private Mock<IPgPaymentService> _pgPaymentServiceMock;
        private Mock<IExchangeRateService> _exchangeRateServiceMock;
        private IPgCostSectionTotalsBuilder _pgTotalsBuilder;

        [SetUp]
        public void Setup()
        {
            PgStageBuilderMock = new Mock<IPgStageBuilder>();
            RuleServiceMock = new Mock<IRuleService>();
            CostStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            AgencyServiceMock = new Mock<IAgencyService>();
            ProjectServiceMock = new Mock<IProjectService>();
            EFContext = EFContextFactory.CreateInMemoryEFContext();
            var currencyService = new Mock<IPgCurrencyService>();
            PgLedgerMaterialCodeServiceMock = new Mock<IPgLedgerMaterialCodeService>();
            CostLineItemServiceMock = new Mock<ICostLineItemService>();
            CostNumberGeneratorServiceMock = new Mock<ICostNumberGeneratorService>();
            CostTemplateVersionServiceMock = new Mock<ICostTemplateVersionService>();
            PermissionServiceMock = new Mock<IPermissionService>();
            _pgCostServiceMock = new Mock<IPgCostService>();
            _pgPaymentServiceMock = new Mock<IPgPaymentService>();
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _pgTotalsBuilder = new PgCostSectionTotalsBuilder();

            CostBuilder = new PgCostBuilder(PgStageBuilderMock.Object,
                RuleServiceMock.Object,
                CostStageRevisionServiceMock.Object,
                AgencyServiceMock.Object,
                ProjectServiceMock.Object,
                EFContext,
                CostNumberGeneratorServiceMock.Object,
                currencyService.Object,
                PgLedgerMaterialCodeServiceMock.Object,
                CostLineItemServiceMock.Object,
                CostTemplateVersionServiceMock.Object,
                PermissionServiceMock.Object,
                _pgCostServiceMock.Object,
                _pgTotalsBuilder,
                _pgPaymentServiceMock.Object,
                _exchangeRateServiceMock.Object
                );
        }

        [Test]
        public void ThrowExceptionForMissingTemplate()
        {
            var templateId = Guid.NewGuid();
            CostTemplateVersionServiceMock.Setup(
                    ctv => ctv.GetLatestTemplateVersion(It.Is<Guid>(id => id == templateId)))
                .ReturnsAsync((CostTemplateVersion)null);

            var user = new CostUser
            {
                Id = Guid.NewGuid(),
                Agency = new Agency()
            };

            Assert.ThrowsAsync<Exception>(() => CostBuilder.CreateCost(user, new CreateCostModel
            {
                TemplateId = templateId,
                StageDetails = new StageDetails
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        AgencyName = "XYZZZZZZ",
                        ProjectId = "123456789",
                    }))
                }
            }), "Template version missing");
        }

        [Test]
        public async Task SubmitCost_Set_ExchangeRateDate_On_OE_State()
        {
            // Arrange
            const decimal costExchangeRate = 0.35m;
            const decimal exchangeRate = 0.25m;
            var dateMin = DateTime.UtcNow;
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                ExchangeRate = costExchangeRate,
                ExchangeRateDate = DateTime.UtcNow.AddDays(-1),
                LatestCostStageRevision = new CostStageRevision
                {
                    Name = CostStages.OriginalEstimate.ToString(),
                    CostStage = new CostStage { Key = CostStages.OriginalEstimate.ToString() },
                    IsPaymentCurrencyLocked = false
                },
                PaymentCurrencyId = Guid.NewGuid()
            };

            EFContext.Cost.Add(cost);
            EFContext.SaveChanges();
            _exchangeRateServiceMock.Setup(er => er.GetCurrentRate(cost.PaymentCurrencyId.Value))
                .ReturnsAsync(new ExchangeRate { Rate = exchangeRate });

            // Act
            await CostBuilder.SubmitCost(cost.Id);

            // Assert
            cost.ExchangeRate.Should().Be(exchangeRate);
            cost.ExchangeRateDate.Should().HaveValue();
            cost.ExchangeRateDate.Should().BeAfter(dateMin);
            cost.ExchangeRateDate.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public async Task SubmitCost_Set_ExchangeRateDate_On_FA_Stage_WhenPaymentCurrencyIsNotLocked()
        {
            // Arrange
            const decimal costExchangeRate = 0.35m;
            const decimal exchangeRate = 0.25m;
            var dateMin = DateTime.UtcNow;
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                ExchangeRate = costExchangeRate,
                ExchangeRateDate = DateTime.UtcNow.AddDays(-1),
                LatestCostStageRevision = new CostStageRevision
                {
                    Name = CostStages.FinalActual.ToString(),
                    CostStage = new CostStage { Key = CostStages.FinalActual.ToString() },
                    IsPaymentCurrencyLocked = false
                },
                PaymentCurrencyId = Guid.NewGuid()
            };

            EFContext.Cost.Add(cost);
            EFContext.SaveChanges();
            _exchangeRateServiceMock.Setup(er => er.GetCurrentRate(cost.PaymentCurrencyId.Value))
                .ReturnsAsync(new ExchangeRate { Rate = exchangeRate });

            // Act
            await CostBuilder.SubmitCost(cost.Id);

            // Assert
            cost.ExchangeRate.Should().Be(exchangeRate);
            cost.ExchangeRateDate.Should().HaveValue();
            cost.ExchangeRateDate.Should().BeAfter(dateMin);
            cost.ExchangeRateDate.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public async Task SubmitCost_Not_Set_ExchangeRateDate_If_PaymentCurrencyIsLocked()
        {
            // Arrange
            const decimal costExchangeRate = 0.35m;
            const decimal exchangeRate = 0.25m;
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                ExchangeRateDate = DateTime.UtcNow.AddDays(-1),
                ExchangeRate = costExchangeRate,
                LatestCostStageRevision = new CostStageRevision
                {
                    Name = CostStages.OriginalEstimate.ToString(),
                    CostStage = new CostStage { Key = CostStages.OriginalEstimate.ToString() },
                    IsPaymentCurrencyLocked = true
                },
                PaymentCurrencyId = Guid.NewGuid()
            };

            EFContext.Cost.Add(cost);
            EFContext.SaveChanges();
            _exchangeRateServiceMock.Setup(er => er.GetCurrentRate(cost.PaymentCurrencyId.Value))
                .ReturnsAsync(new ExchangeRate { Rate = exchangeRate } );

            // Act
            await CostBuilder.SubmitCost(cost.Id);


            // Assert
            cost.ExchangeRateDate.Should().Be(cost.ExchangeRateDate);
            cost.ExchangeRate.Should().Be(costExchangeRate);
        }
    }
}

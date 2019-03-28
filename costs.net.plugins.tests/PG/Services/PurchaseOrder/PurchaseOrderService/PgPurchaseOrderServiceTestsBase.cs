namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using core.Events.Cost;
    using core.Extensions;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.CustomData;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services;
    using plugins.PG.Services.PurchaseOrder;
    using Serilog;

    public class PgPurchaseOrderServiceTestsBase
    {
        protected const string FrontEndUrl = "http://adstream.com:8882/";
        protected readonly Guid _eurId = new Guid("3C331A29-D7B6-4259-AE5C-10ECDF015664");
        private readonly Guid _gbpId = new Guid("EAA47714-018C-4220-9262-33F776A6D90C");
        protected readonly Guid _usdId = new Guid("59CED7E1-0C9D-4BCC-9FBF-B890EB001076");

        private Mock<IOptions<AppSettings>> _appSettingsMock;
        protected Mock<ICustomObjectDataService> _customDataServiceMock;
        protected Mock<IPgLedgerMaterialCodeService> _ledgerMaterialCodeServiceMock;
        private IPgCurrencyService _pgCurrencyService;
        private Mock<EFContext> _efContext;
        private Mock<ILogger> _loggerMock;
        protected Mock<IPgPaymentService> _paymentServiceMock;
        protected PgPurchaseOrderService PgPurchaseOrderService;

        protected static readonly CostStageRevisionStatus[] NonCancelledAndNotApprovedStatuses = Enum.GetValues(typeof(CostStageRevisionStatus))
            .Cast<CostStageRevisionStatus>()
            .Where(c => c != CostStageRevisionStatus.PendingCancellation && c != CostStageRevisionStatus.Approved)
            .ToArray();

        [SetUp]
        public void Init()
        {
            _efContext = new Mock<EFContext>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _customDataServiceMock = new Mock<ICustomObjectDataService>();
            _ledgerMaterialCodeServiceMock = new Mock<IPgLedgerMaterialCodeService>();
            _appSettingsMock.Setup(s => s.Value).Returns(new AppSettings { FrontendUrl = FrontEndUrl });
            _ledgerMaterialCodeServiceMock.Setup(c => c.GetLedgerMaterialCodes(It.IsAny<Guid>()))
                .ReturnsAsync(new PgLedgerMaterialCodeModel());
            _paymentServiceMock = new Mock<IPgPaymentService>();
            var costExchangeRateServiceMock = new Mock<ICostExchangeRateService>();

            _pgCurrencyService = new PgCurrencyService(_efContext.Object);
            _loggerMock = new Mock<ILogger>();

            PgPurchaseOrderService = new PgPurchaseOrderService(
                _efContext.Object,
                _appSettingsMock.Object,
                _ledgerMaterialCodeServiceMock.Object,
                _customDataServiceMock.Object,
                _pgCurrencyService,
                _loggerMock.Object,
                _paymentServiceMock.Object,
                costExchangeRateServiceMock.Object
                );

            _customDataServiceMock.Setup(ds => ds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), CustomObjectDataKeys.PgPurchaseOrderResponse))
                .ReturnsAsync(new PgPurchaseOrderResponse());

            SetupCurrencies();
        }

        protected Cost SetupPurchaseOrderView(
            Guid costId,
            IDictionary<string, dynamic> stageDetails = null,
            IDictionary<string, dynamic> productionDetailsData = null,
            string brandName = null,
            string costNumber = null,
            string tNumber = null,
            string requisitionerEmail = null,
            CostStages costStage = CostStages.OriginalEstimate,
            string[] agencyLabels = null,
            CostType costType = CostType.Production,
            bool isExternalPurchase = false
            )
        {
            var stage = new CostStage
            {
                Key = costStage.ToString(),
                Name = costStage.ToString()
            };

            var costStageRevision = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                StageDetails = new CustomFormData
                {
                    Data = JsonConvert.SerializeObject(stageDetails ?? new Dictionary<string, dynamic>())
                },
                ProductDetails = new CustomFormData
                {
                    Data = JsonConvert.SerializeObject(productionDetailsData ?? new Dictionary<string, dynamic>())
                },
                CostStage = stage,
                Approvals = new List<Approval>
                {
                    new Approval
                    {
                        Type = ApprovalType.Brand,
                        Requisitioners = new List<Requisitioner>
                        {
                            new Requisitioner
                            {
                                CostUser = new CostUser
                                {
                                    Email = requisitionerEmail,
                                    FederationId = tNumber
                                }
                            }
                        },
                    }
                }
            };
            stage.CostStageRevisions = new List<CostStageRevision> { costStageRevision };

            var cost = new Cost
            {
                Id = costId,
                CostNumber = costNumber,
                CostType = costType,
                Project = new Project
                {
                    Brand = new Brand { Name = brandName }
                },
                LatestCostStageRevision = costStageRevision,
                CostStages = new List<CostStage> { stage },
                Parent = new AbstractType
                {
                    Agency = new Agency { Labels = agencyLabels ?? new string[0] }
                },
                IsExternalPurchases = isExternalPurchase
            };

            var costDbSetMock = _efContext.MockAsyncQueryable(new[] { cost }.AsQueryable(), d => d.Cost);
            costDbSetMock.Setup(c => c.FindAsync(costId)).ReturnsAsync(cost);

            _efContext.MockAsyncQueryable(new List<CostLineItem>().AsQueryable(), d => d.CostLineItem);

            return cost;
        }

        protected CostStageRevisionStatusChanged GetCostRevisionStatusChanged(CostStageRevisionStatus status)
        {
            return new CostStageRevisionStatusChanged
            {
                AggregateId = Guid.NewGuid(),
                Status = status,
                TimeStamp = DateTime.UtcNow,
                CostStageRevisionId = Guid.NewGuid()
            };
        }

        protected void SetupCustomObjectData<T>(string name, Dictionary<string, dynamic> data)
        {
            _customDataServiceMock.Setup(s => s.GetCustomData<T>(It.IsAny<Guid>(), name)).ReturnsAsync(data.ToModel<T>());
        }

        protected void SetupCurrencies(string vendorCurrency = null)
        {
            var gbp = new Currency
            {
                Code = "GBP",
                DefaultCurrency = false,
                Id = _gbpId
            };
            var usd = new Currency
            {
                Code = "USD",
                DefaultCurrency = true,
                Id = _usdId
            };
            var eur = new Currency
            {
                Code = "EUR",
                DefaultCurrency = false,
                Id = _eurId
            };
            var currencies = new List<Currency> { gbp, usd, eur };

            if (!string.IsNullOrEmpty(vendorCurrency))
            {
                var dpVendorId = new Guid();
                var vendors = new List<Vendor>
                {
                    new Vendor
                    {
                        Id = dpVendorId,
                        Categories = new List<VendorCategory>
                        {
                            new VendorCategory
                            {
                                Currency = currencies.FirstOrDefault(x => x.Code == vendorCurrency),
                                DefaultCurrencyId = _usdId
                            }
                        }
                    }
                };
                _efContext.MockAsyncQueryable(vendors.AsQueryable(), c => c.Vendor);
            }

            _efContext.MockAsyncQueryable(currencies.AsQueryable(), c => c.Currency);
        }
    }
}
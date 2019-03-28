namespace costs.net.integration.tests.Plugins.PG
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Builders.Rules;
    using core.Builders.Workflow;
    using core.Events.Cost;
    using core.Models;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.Currencies;
    using core.Services.CustomData;
    using core.Services.Payments;
    using core.Services.Rules;
    using core.Services.Workflow;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Views;
    using Microsoft.Extensions.Options;
    using Serilog;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Builders.Payments;
    using plugins.PG.Builders.Workflow;
    using plugins.PG.Form;
    using plugins.PG.Models;
    using plugins.PG.Models.Rules;
    using plugins.PG.Services;
    using plugins.PG.Services.Costs;
    using plugins.PG.Services.PurchaseOrder;

    public class PaymentRuleIntegrationTestBase : BaseIntegrationTest
    {
        protected PgPurchaseOrderService _purchaseOrderService;
        protected string _contentType;
        protected Cost _cost;
        protected List<CostLineItemView> _costLineItems;
        protected Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        protected Mock<ICustomObjectDataService> _customDataServiceMock;
        protected Mock<ILogger> _loggerMock;
        protected Mock<ICurrencyService> _currencyService;
        protected Mock<ICostExchangeRateService> _costExchangeRateServiceMock;
        protected Mock<IPgLedgerMaterialCodeService> _pgLedgerMaterialCodeServiceMock;
        protected Mock<IOptions<AppSettings>> _appSettingsMock;
        protected Mock<IPgCostService> _pgCostServiceMock;

        protected PgPaymentDetails _paymentDetailsData;
        protected PgProductionDetailsForm _productionDetails;
        protected EFContext _efContext;
        protected Mock<ICustomObjectDataService> _customObjectDataService;
        protected string _productionType;
        protected CostStageRevision _revision;
        protected CostStageRevision _previousRevision;
        protected CostStageRevisionStatusChanged _costApprovedEvent;

        protected CostStage _stage;
        protected PgStageDetailsForm _stageDetails;

        protected RuleService Service;
        protected IPgCurrencyService _pgCurrencyService;
        protected IPgPaymentService _pgPaymentService;
        protected IPaymentService _paymentService;
        protected Guid? _vendorWithMultipleRulesId;

        protected Guid eurId = new Guid("3C331A29-D7B6-4259-AE5C-10ECDF015664");
        protected Guid gbpId = new Guid("EAA47714-018C-4220-9262-33F776A6D90C");
        protected Guid usdId = new Guid("59CED7E1-0C9D-4BCC-9FBF-B890EB001076");

        protected Guid _costStageRevisionId;

        private IPgCostSectionTotalsBuilder _pgTotalsBuilder;
        private IPgCostStageRevisionTotalPaymentsBuilder _pgTotalPaymentsBuilder;

        [SetUp]
        public void Init()
        {
            var ruleEngine = GetService<IRuleEngine>();
            var efContext = GetService<EFContext>();
            var ruleBuilders = GetService<IEnumerable<Lazy<IVendorRuleBuilder, PluginMetadata>>>();
            var ruleServices = GetService<IEnumerable<Lazy<IPluginRuleService, PluginMetadata>>>();

            Service = new RuleService(ruleBuilders, ruleServices, ruleEngine, efContext);

            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _customDataServiceMock = new Mock<ICustomObjectDataService>();
            _loggerMock = new Mock<ILogger>();
            _currencyService = new Mock<ICurrencyService>();
            _costExchangeRateServiceMock = new Mock<ICostExchangeRateService>();
            _pgLedgerMaterialCodeServiceMock = new Mock<IPgLedgerMaterialCodeService>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _pgCostServiceMock = new Mock<IPgCostService>();

            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _pgCurrencyService = new PgCurrencyService(_efContext);
            _customObjectDataService = new Mock<ICustomObjectDataService>();

            var pgStageBuilder = new PgStageBuilder(Service,
                _efContext,
                _costStageRevisionServiceMock.Object,
                _pgCostServiceMock.Object
                );

            var stageBuilders = new List<Lazy<IStageBuilder, PluginMetadata>>
            {
                new Lazy<IStageBuilder, PluginMetadata>(
                    () => pgStageBuilder,
                    new PluginMetadata { BuType = BuType.Pg }
                )
            };

            var stageService = new StageService(stageBuilders);

            _pgTotalsBuilder = new PgCostSectionTotalsBuilder();
            _pgTotalPaymentsBuilder = new PgCostStageRevisionTotalPaymentsBuilder();

            var paymentService = new PgPaymentService(
                _efContext,
                _costStageRevisionServiceMock.Object,
                Service,
                stageService,
                _pgCurrencyService,
                _pgTotalsBuilder,
                _pgTotalPaymentsBuilder,
                _customObjectDataService.Object
                );
            _pgPaymentService = paymentService;
            _paymentService = paymentService;

            _purchaseOrderService = new PgPurchaseOrderService(
                _efContext,
                _appSettingsMock.Object,
                _pgLedgerMaterialCodeServiceMock.Object,
                _customDataServiceMock.Object,
                _pgCurrencyService,
                _loggerMock.Object,
                _pgPaymentService,
                _costExchangeRateServiceMock.Object);
        }

        protected async Task<Guid> CreateVendorVideoProductionRule()
        {
            const string vendorName = "Vendor1_Production_Rule";
            const string audioCategory = "Video company";
            var sapVendorCode = "S_ALR_87012011" + DateTime.Now.Millisecond;
            const string EuropeRegion = Constants.BudgetRegion.Europe;
            const string contentTypeVideo = Constants.ContentType.Video;
            const string productionType = Constants.ProductionType.FullProduction;

            var vendor = new Vendor
            {
                Name = vendorName,
                SapVendor = sapVendorCode,
                Categories = new List<VendorCategory>()
            };

            var audioVendorCategory = new VendorCategory
            {
                Name = audioCategory,
                Vendor = vendor,
                HasDirectPayment = true,
                Currency = new Currency { Code = "USD", Description = "USD", Symbol = "s" },
            };

            var vendorRule1 = new VendorRule
            {
                Rule = GetRule(vendorName, false, EuropeRegion, contentTypeVideo, productionType, 0m,
                    new PgPaymentRuleDefinition
                    {
                        DetailedSplit = true,
                        Splits = new[]
                        {
                            new PgPaymentRuleDefinitionSplit
                            {
                                FASplit = 1m,
                                FPSplit=1m,
                                OESplit = 0m,
                                CostTotalName = Constants.CostSection.Production
                            }
                        }
                    }),
                VendorCategory = audioVendorCategory
            };

            EFContext.VendorRule.Add(vendorRule1);
            await EFContext.SaveChangesAsync();
            return vendorRule1.VendorCategory.VendorId;
        }

        protected async Task<Guid> CreateVendor()
        {
            const string vendorName = "Vendor1_MultipleCategories_MultipleRules";
            const string musicCategory = "Music company";
            const string audioCategory = "Audio company";
            var sapVendorCode = "S_ALR_87012011" + DateTime.Now.Millisecond;
            const string chinaRegion = Constants.BudgetRegion.China;
            const string europeRegion = Constants.BudgetRegion.Europe;
            const string contentType1 = Constants.ContentType.Video;
            const string contentType2 = Constants.ContentType.Audio;
            const string productionType = Constants.ProductionType.FullProduction;

            var vendor = new Vendor
            {
                Name = vendorName,
                SapVendor = sapVendorCode,
                Categories = new List<VendorCategory>()
            };
            var musicVendorCategory = new VendorCategory
            {
                Name = musicCategory,
                Vendor = vendor,
                HasDirectPayment = true,
                Currency = new Currency { Code = "USD", Description = "USD", Symbol = "s" },
            };

            var audioVendorCategory = new VendorCategory
            {
                Name = audioCategory,
                Vendor = vendor,
                HasDirectPayment = true
            };

            var vendorRule1 = new VendorRule
            {
                Rule = GetRule(vendorName, false, chinaRegion, contentType1, productionType, 10000m,
                    new PgPaymentRuleDefinition
                    {
                        Splits = new[]
                        {
                            new PgPaymentRuleDefinitionSplit
                            {
                                FASplit = 0.7m,
                                OESplit = 0.2m,
                                CostTotalName = Constants.CostSection.CostTotal
                            }
                        }
                    }),
                VendorCategory = musicVendorCategory
            };
            var vendorRule2 = new VendorRule
            {
                Rule = GetRule(vendorName, false, europeRegion, contentType2, productionType, 10000m,
                    new PgPaymentRuleDefinition
                    {
                        Splits = new[]
                        {
                            new PgPaymentRuleDefinitionSplit
                            {
                                FASplit = 0.7m,
                                OESplit = 0.4m,
                                CostTotalName = Constants.CostSection.CostTotal
                            }
                        }
                    }),
                VendorCategory = audioVendorCategory
            };
            var vendorRule3 = new VendorRule
            {
                Rule = GetRule(vendorName, false, europeRegion, contentType2, productionType, 50000m,
                    new PgPaymentRuleDefinition
                    {
                        Splits = new[]
                        {
                            new PgPaymentRuleDefinitionSplit
                            {
                                FASplit = 0.7m,
                                OESplit = 0.5m,
                                CostTotalName = Constants.CostSection.CostTotal
                            }
                        }
                    }),
                VendorCategory = audioVendorCategory
            };

            var vendorRule4 = new VendorRule
            {
                Rule = GetRule(vendorName, false, europeRegion, contentType2, productionType, 10000m,
                    new PgPaymentRuleDefinition
                    {
                        Splits = new[]
                        {
                            new PgPaymentRuleDefinitionSplit
                            {
                                FASplit = 0.7m,
                                OESplit = 0.3m,
                                CostTotalName = Constants.CostSection.CostTotal
                            }
                        }
                    }),
                VendorCategory = musicVendorCategory
            };
            EFContext.VendorRule.Add(vendorRule1);
            EFContext.VendorRule.Add(vendorRule2);
            EFContext.VendorRule.Add(vendorRule3);
            EFContext.VendorRule.Add(vendorRule4);

            await EFContext.SaveChangesAsync();
            return vendorRule1.VendorCategory.VendorId;
        }


        protected void InitData(
            bool isAipe = false,
            string stageKey = "Aipe",
            string budgetRegion = "AAK (Asia)",
            List<CostLineItemView> items = null,
            string targetBudget = "0",
            List<CostStageRevisionPaymentTotal> payments = null,
            string contentType = Constants.ContentType.Photography,
            string productionType = Constants.ProductionType.FullProduction,
            CostType costType = CostType.Production,
            string agencyCurrency = "USD",
            Guid? dpvCurrency = null,
            Guid? dpvId = null,
            string vendorCategory = null
            )
        {
            SetupCurrencies();

            _costStageRevisionId = Guid.NewGuid();
            var previousCostStageRevisionId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costStageId = Guid.NewGuid();
            var paymentCurrencyId = dpvCurrency ?? _efContext.Currency.FirstOrDefault(c => c.Code == agencyCurrency)?.Id;

            _stage = new CostStage { Key = stageKey, Id = costStageId };
            _contentType = contentType;
            _productionType = productionType;
            _stageDetails = GetStageDetails(isAipe, budgetRegion, targetBudget, costType, agencyCurrency);
            _productionDetails = GetProductionDetails(dpvCurrency, dpvId, vendorCategory);
            _revision = new CostStageRevision
            {
                Id = _costStageRevisionId,
                StageDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_stageDetails) },
                ProductDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_productionDetails) },
                CostStage = _stage,
                Approvals = new List<Approval>()
            };

            _cost = new Cost
            {
                Id = costId,
                CostType = costType,
                LatestCostStageRevisionId = _costStageRevisionId,
                LatestCostStageRevision = _revision,
                Project = new Project(),
                Parent = new AbstractType
                {
                    Agency = new Agency()
                },
                PaymentCurrencyId = paymentCurrencyId,
                ExchangeRate = _efContext.ExchangeRate.FirstOrDefault(er => er.FromCurrency == paymentCurrencyId)?.Rate
            };
            _stage.Cost = _cost;

            var previousRevision = new CostStageRevision { Id = previousCostStageRevisionId };
            _costApprovedEvent = new CostStageRevisionStatusChanged(_cost.Id, _revision.Id, CostStageRevisionStatus.Approved, BuType.Pg);

            _paymentDetailsData = new PgPaymentDetails();
            _costLineItems = new List<CostLineItemView>();
            if (items != null)
            {
                _costLineItems.AddRange(items);
            }

            var paymentsList = new List<CostStageRevisionPaymentTotal>();
            if (payments != null)
            {
                paymentsList.AddRange(payments);
            }

            _costStageRevisionServiceMock.Setup(csr => csr.GetRevisionById(_costStageRevisionId)).ReturnsAsync(_revision);
            _costStageRevisionServiceMock.Setup(csr => csr.GetPreviousRevision(costStageId)).ReturnsAsync(previousRevision);

            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costStageRevisionId)))
                .Returns(_stageDetails);

            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetProductionDetails<PgProductionDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costStageRevisionId)))
                .Returns(_productionDetails);

            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(_costStageRevisionId, It.IsAny<bool>())).ReturnsAsync((List<CostStageRevisionPaymentTotal>)null);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(previousCostStageRevisionId, It.IsAny<bool>())).ReturnsAsync(paymentsList);

            _costStageRevisionServiceMock.Setup(csr => csr.GetAllCostPaymentTotals(costId, costStageId)).ReturnsAsync(paymentsList);

            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPaymentDetails>(_costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .ReturnsAsync(_paymentDetailsData);

            _costStageRevisionServiceMock.Setup(csr => csr.GetCostLineItems(_costStageRevisionId)).ReturnsAsync(_costLineItems);
            _efContext.Cost.Add(_cost);
            _efContext.SaveChanges();
        }

        protected void InitDataForReopenedFA(
            bool isAipe = false,
            string stageKey = "Aipe",
            string budgetRegion = "AAK (Asia)",
            List<CostLineItemView> items = null,
            List<CostLineItemView> previousFAItems = null,
            string targetBudget = "0",
            List<CostStageRevisionPaymentTotal> payments = null,
            List<CostStageRevisionPaymentTotal> previousFAPayments = null,
            string contentType = Constants.ContentType.Video,
            string productionType = Constants.ProductionType.FullProduction,
            CostType costType = CostType.Production,
            string agencyCurrency = "USD",
            Guid? dpvCurrency = null,
            Guid? dpvId = null,
            string vendorCategory = null
            )
        {
            SetupCurrencies();

            var previousCostStageRevisionId = Guid.NewGuid();
            _costStageRevisionId = Guid.NewGuid();
            var _costPreviousStageRevisionId = Guid.NewGuid();

            var costId = Guid.NewGuid();
            var costStageId = Guid.NewGuid();
            var paymentCurrencyId = dpvCurrency ?? _efContext.Currency.FirstOrDefault(c => c.Code == agencyCurrency)?.Id;

            _stage = new CostStage { Key = stageKey, Id = costStageId };
            _contentType = contentType;
            _productionType = productionType;
            _stageDetails = GetStageDetails(isAipe, budgetRegion, targetBudget, costType, agencyCurrency);
            _productionDetails = GetProductionDetails(dpvCurrency, dpvId, vendorCategory);
            _revision = new CostStageRevision
            {
                Id = _costStageRevisionId,
                StageDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_stageDetails) },
                ProductDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_productionDetails) },
                CostStage = _stage,
                Status = CostStageRevisionStatus.Approved,
                Approvals = new List<Approval>()
            };

            _previousRevision = new CostStageRevision
            {
                Id = _costPreviousStageRevisionId,
                StageDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_stageDetails) },
                ProductDetails = new CustomFormData { Data = JsonConvert.SerializeObject(_productionDetails) },
                CostStage = _stage,
                Status = CostStageRevisionStatus.Approved,
                Approvals = new List<Approval>()
            };

            _cost = new Cost
            {
                Id = costId,
                CostType = costType,
                LatestCostStageRevisionId = _costStageRevisionId,
                LatestCostStageRevision = _revision,

                Project = new Project(),
                Parent = new AbstractType
                {
                    Agency = new Agency()
                },
                PaymentCurrencyId = paymentCurrencyId,
                ExchangeRate = _efContext.ExchangeRate.FirstOrDefault(er => er.FromCurrency == paymentCurrencyId)?.Rate
            };
            _stage.Cost = _cost;
            _stage.Name = "Final Actual";

            var previousRevision = new CostStageRevision { Id = previousCostStageRevisionId };

            //add previous stage revision to the stage
            _stage.CostStageRevisions.Add(_previousRevision);

            _costApprovedEvent = new CostStageRevisionStatusChanged(_cost.Id, _previousRevision.Id, CostStageRevisionStatus.Approved, BuType.Pg);
            _costApprovedEvent = new CostStageRevisionStatusChanged(_cost.Id, _revision.Id, CostStageRevisionStatus.Approved, BuType.Pg);

            _paymentDetailsData = new PgPaymentDetails();
            _costLineItems = new List<CostLineItemView>();
            if (items != null)
            {
                _costLineItems.AddRange(items);
            }
            var _previousCostLineItems = new List<CostLineItemView>();
            if (items != null)
            {
                _previousCostLineItems.AddRange(previousFAItems);
            }

            var paymentsList = new List<CostStageRevisionPaymentTotal>();
            if (payments != null)
            {
                foreach (var payment in payments)
                {
                    payment.CostStageRevision = _revision;
                }
                paymentsList.AddRange(payments);
            }
            var previousPaymentsList = new List<CostStageRevisionPaymentTotal>();
            if (previousFAPayments != null)
            {
                foreach (var payment in previousFAPayments)
                {
                    payment.CostStageRevision = _previousRevision;
                }
                previousPaymentsList.AddRange(previousFAPayments);
            }
            //set upp last stage revision data
            _costStageRevisionServiceMock.Setup(csr => csr.GetRevisionById(_costPreviousStageRevisionId)).ReturnsAsync(_previousRevision);
            _costStageRevisionServiceMock.Setup(csr => csr.GetPreviousRevision(costStageId)).ReturnsAsync(previousRevision);
            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costPreviousStageRevisionId)))
                .Returns(_stageDetails);
            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetProductionDetails<PgProductionDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costPreviousStageRevisionId)))
                .Returns(_productionDetails);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(_costPreviousStageRevisionId, It.IsAny<bool>())).ReturnsAsync((List<CostStageRevisionPaymentTotal>)null);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(previousCostStageRevisionId, It.IsAny<bool>())).ReturnsAsync(previousPaymentsList);
            _costStageRevisionServiceMock.Setup(csr => csr.GetAllCostPaymentTotals(costId, costStageId)).ReturnsAsync(previousPaymentsList);
            _costStageRevisionServiceMock.Setup(csr => csr.GetAllCostPaymentTotalsFinalActual(costId, costStageId)).ReturnsAsync(previousPaymentsList);
            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPaymentDetails>(_costPreviousStageRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .ReturnsAsync(_paymentDetailsData);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostLineItems(_costPreviousStageRevisionId)).ReturnsAsync(_previousCostLineItems);

            //set up latest stage revision data
            _costStageRevisionServiceMock.Setup(csr => csr.GetRevisionById(_costStageRevisionId)).ReturnsAsync(_revision);
            _costStageRevisionServiceMock.Setup(csr => csr.GetPreviousRevision(costStageId)).ReturnsAsync(previousRevision);
            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costStageRevisionId)))
                .Returns(_stageDetails);
            _costStageRevisionServiceMock.Setup(csr =>
                csr.GetProductionDetails<PgProductionDetailsForm>(It.Is<CostStageRevision>(r => r.Id == _costStageRevisionId)))
                .Returns(_productionDetails);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(_costStageRevisionId, It.IsAny<bool>())).ReturnsAsync((List<CostStageRevisionPaymentTotal>)null);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostStageRevisionPaymentTotals(previousCostStageRevisionId, It.IsAny<bool>())).ReturnsAsync(paymentsList);
            _costStageRevisionServiceMock.Setup(csr => csr.GetAllCostPaymentTotals(costId, costStageId)).ReturnsAsync(paymentsList);
            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPaymentDetails>(_costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .ReturnsAsync(_paymentDetailsData);
            _costStageRevisionServiceMock.Setup(csr => csr.GetCostLineItems(_costStageRevisionId)).ReturnsAsync(_costLineItems);


            _efContext.Cost.Add(_cost);
            _efContext.SaveChanges();
        }

        private PgProductionDetailsForm GetProductionDetails(Guid? dpvCurrency, Guid? dpvId, string vendorCategory = null)
        {
            var productionDetails = new PgProductionDetailsForm();
            if (dpvCurrency.HasValue)
            {
                productionDetails.DirectPaymentVendor = new PgProductionDetailsForm.Vendor { CurrencyId = dpvCurrency };
            }

            if (dpvId.HasValue)
            {
                if (productionDetails.DirectPaymentVendor == null)
                {
                    productionDetails.DirectPaymentVendor = new PgProductionDetailsForm.Vendor();
                }

                productionDetails.DirectPaymentVendor.Id = dpvId.Value;
                productionDetails.DirectPaymentVendor.ProductionCategory = vendorCategory;
            }

            return productionDetails;
        }

        protected static Rule GetRule(string vendorName, bool isAipe, string budgetRegion, string contentType, string productionType, decimal total, PgPaymentRuleDefinition ruleDefinition)
        {
            var criterion = new RuleCriterion
            {
                Operator = ExpressionType.And.ToString()
            };

            criterion.Children.Add(new RuleCriterion
            {
                FieldName = nameof(PgPaymentRule.BudgetRegion),
                Operator = ExpressionType.Equal.ToString(),
                TargetValue = budgetRegion
            });
            if (!string.IsNullOrEmpty(contentType) && contentType != "All")
            {
                criterion.Children.Add(new RuleCriterion
                {
                    FieldName = nameof(PgPaymentRule.ContentType),
                    Operator = ExpressionType.Equal.ToString(),
                    TargetValue = contentType
                });
            }

            if (!string.IsNullOrEmpty(productionType) && productionType != "All")
            {
                criterion.Children.Add(new RuleCriterion
                {
                    FieldName = nameof(PgPaymentRule.ProductionType),
                    Operator = ExpressionType.Equal.ToString(),
                    TargetValue = productionType
                });
            }

            criterion.Children.Add(new RuleCriterion
            {
                FieldName = nameof(PgPaymentRule.IsAIPE),
                Operator = ExpressionType.Equal.ToString(),
                TargetValue = isAipe.ToString()
            });
            criterion.Children.Add(new RuleCriterion
            {
                FieldName = nameof(PgPaymentRule.TotalCostAmount),
                Operator = ExpressionType.GreaterThanOrEqual.ToString(),
                TargetValue = total.ToString()
            });

            var rule = new Rule
            {
                Name = $"{vendorName}_vendor test rule_{Guid.NewGuid()}",
                Criterion = criterion,
                Definition = JsonConvert.SerializeObject(ruleDefinition),
                Type = RuleType.VendorPayment
            };
            return rule;
        }

        private PgStageDetailsForm GetStageDetails(bool isAipe, string budgetRegion, string targetBudget, CostType costType, string agencyCurrency)
        {
            var stageDetails = new PgStageDetailsForm
            {
                BudgetRegion = new AbstractTypeValue { Key = budgetRegion },
                InitialBudget = decimal.Parse(targetBudget),
                IsAIPE = isAipe,
                CostType = costType.ToString(),
                AgencyCurrency = agencyCurrency
            };

            if (costType == CostType.Production)
            {
                stageDetails.ContentType = new DictionaryValue { Key = _contentType };
                stageDetails.ProductionType = new DictionaryValue { Key = _productionType };
            }
            else
            {
                stageDetails.UsageBuyoutType = new DictionaryValue { Key = _contentType };
            }

            return stageDetails;
        }

        private void SetupCurrencies()
        {
            var gbp = new Currency
            {
                Code = "GBP",
                DefaultCurrency = false,
                Id = gbpId
            };
            var usd = new Currency
            {
                Code = "USD",
                DefaultCurrency = true,
                Id = usdId
            };
            var eur = new Currency
            {
                Code = "EUR",
                DefaultCurrency = false,
                Id = eurId
            };
            var currencies = new List<Currency> { gbp, usd, eur };
            var gbpExchangeRate = new ExchangeRate
            {
                FromCurrency = gbpId,
                ToCurrency = usdId,
                Rate = 2
            };
            var eurExchangeRate = new ExchangeRate
            {
                FromCurrency = eurId,
                ToCurrency = usdId,
                Rate = 5
            };
            var defaultExchangeRates = new List<ExchangeRate> { gbpExchangeRate, eurExchangeRate };

            _currencyService.Setup(x => x.GetCurrency("GBP")).ReturnsAsync(gbp);
            _currencyService.Setup(x => x.GetCurrency("USD")).ReturnsAsync(usd);
            _currencyService.Setup(x => x.GetCurrency("EUR")).ReturnsAsync(eur);
            var dpVendorId = new Guid();
            var vendors = new List<Vendor>
            {
                new Vendor
                {
                    Id = dpVendorId
                }
            };

            _efContext.Currency.AddRange(currencies);
            _efContext.ExchangeRate.AddRange(defaultExchangeRates);
            _efContext.Vendor.AddRange(vendors);
            _efContext.SaveChanges();
        }
    }
}

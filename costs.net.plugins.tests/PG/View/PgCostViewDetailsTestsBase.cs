
namespace costs.net.plugins.tests.PG.Extensions
{
    using AutoMapper;
    using costs.net.core.Builders;
    using costs.net.core.Builders.Workflow;
    using costs.net.core.Mapping;
    using costs.net.core.Models;
    using costs.net.core.Models.User;
    using costs.net.core.Services;
    using costs.net.core.Services.Costs;
    using costs.net.core.Services.CostTemplate;
    using costs.net.core.Services.Rules;
    using costs.net.dataAccess;
    using costs.net.plugins.PG.Builders.Search;
    using costs.net.plugins.PG.Builders.Workflow;
    using costs.net.plugins.PG.Mapping;
    using costs.net.plugins.PG.Services.PurchaseOrder;
    using costs.net.tests.common.Stubs.EFContext;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Extensions;
    using plugins.PG.Form;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class PgCostViewDetailsTestsBase
    {
        protected PgCostViewDetails CostViewDetails;
        protected Mock<IStageBuilder> PgStageBuilderMock;
        private IMapper _mapper;
        protected Mock<IPermissionService> PermissionServiceMock;
        protected Mock<IRuleService> RuleServiceMock;
        protected Mock<IPgPurchaseOrderService> PurchaseOrderServiceMock;
        protected Mock<ICostService> CostServiceMock;

        protected EFContext EFContext;
        protected UserIdentity UserIdentity;
        protected Mock<ICostTemplateService> CostTemplateServiceMock;

        protected Guid CostId;
        protected Guid RevisionId;
        protected Guid CurrencyUsdId;

        [SetUp]
        public void Setup()
        {
            EFContext = EFContextFactory.CreateInMemoryEFContext();
            PgStageBuilderMock = new Mock<IStageBuilder>();
            RuleServiceMock = new Mock<IRuleService>();
            PermissionServiceMock = new Mock<IPermissionService>();
            PurchaseOrderServiceMock = new Mock<IPgPurchaseOrderService>();
            CostTemplateServiceMock = new Mock<ICostTemplateService>();
            CostServiceMock = new Mock<ICostService>();
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<UserProfile>();
                m.AddProfile<SupportingDocumentProfile>();
                m.AddProfile<CostModelProfile>();
                m.AddProfile<CostSearchProfile>();
            }));

            CostViewDetails = new PgCostViewDetails(_mapper,
                EFContext,
                PermissionServiceMock.Object,
                new[]
                {
                    new Lazy<IStageBuilder, PluginMetadata>(() => PgStageBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
                },
                RuleServiceMock.Object,
                PurchaseOrderServiceMock.Object,
                CostTemplateServiceMock.Object,
                CostServiceMock.Object);

            CostId = Guid.NewGuid();
            RevisionId = Guid.NewGuid();
            CurrencyUsdId = Guid.NewGuid();
            UserIdentity = new UserIdentity()
            {
                Id = Guid.NewGuid()
            };
            SetupData();
        }

        public void InitData()
        {
            var cost = new Cost();
            var paymentCurrency = new Currency();
            var project = new Project();

            var agency = new Agency();
            var country = new Country();
            var owner = new CostOwner();
            var notificationSubscribers = new NotificationSubscriber();
            var userBusinessRoles = new List<UserBusinessRole>();


            var costStages = new List<CostStage>();
            var costStageRevisions = new List<CostStageRevision>();
            var selectRevisionTravelCost = new TravelCost();
            var oeRevisionTravelCost = new TravelCost();
            var region = new Region();
            var payments = new List<CostStageRevisionPaymentTotal>();
            var cli = new List<CostLineItem>();
            var stageDetails = new CustomObjectData();
            var productionDetails = new CustomObjectData();
            var supportingDocuments = new List<SupportingDocument>();
            var lstObjectData = new List<CustomObjectData>();
            var costFormDetails = new List<CostFormDetails>();
            var customFormData = new CustomFormData();
            var approvals = new List<Approval>();
            var approvalMembers = new List<ApprovalMember>();
        }

        private void SetupBusinessRoles()
        {
            var ipmBusinessRole = new BusinessRole()
            {
                Key = "Integrated Production Manager",
                Value = "Integrated Production Manager",
            };
            var brandBusinessRole = new BusinessRole()
            {
                Key = "Brand Manager",
                Value = "Brand Management Approver"
            };
            var agencyAdmin = new BusinessRole()
            {
                Key = "Agency Admin",
                Value = "Agency Admin"
            };
            EFContext.BusinessRole.Add(ipmBusinessRole);
            EFContext.BusinessRole.Add(brandBusinessRole);
            EFContext.BusinessRole.Add(agencyAdmin);
            EFContext.SaveChanges();
        }

        private TravelCost SetUpTravelCost()
        {
            return new TravelCost
            {
                Id = Guid.NewGuid(),
                Region = new GeoRegion
                {
                    Id = Guid.NewGuid(),
                    Key = "North America",
                    Value = "North America"
                },
                Country = new Country
                {
                    Id = Guid.NewGuid(),
                    Name = "Afghanistan",
                    Iso = "AF"
                }
            };
        }

        private List<CostStageRevisionPaymentTotal> SetUpCostStageRevisionPaymentTotal()
        {
            return new List<CostStageRevisionPaymentTotal>() { };
        }

        private List<CostLineItem> SetUpCostLineItem(Guid costStageRevisionId)
        {
            return new List<CostLineItem>() {
                new CostLineItem { Id = Guid.NewGuid(), CostStageRevisionId = costStageRevisionId, Name = "Test", ValueInLocalCurrency = 10000, ValueInDefaultCurrency = 15000, LocalCurrencyId = CurrencyUsdId },
                new CostLineItem { Id = Guid.NewGuid(), CostStageRevisionId = costStageRevisionId, Name = "Test1", ValueInLocalCurrency = 10000, ValueInDefaultCurrency = 15000, LocalCurrencyId = CurrencyUsdId }
            };
        }

        private List<SupportingDocument> SetUpSupportingDocuments()
        {
            return new List<SupportingDocument>() { };
        }

        private List<CustomObjectData> SetUpCustomObjectData(Guid costStageRevisionId)
        {
            return new List<CustomObjectData>() {
                new CustomObjectData { Id = Guid.NewGuid(), ObjectId = costStageRevisionId, Name = "PaymentDetails", Data = "{\"grNumber\": \"\", \"ioNumber\": \"4001561794\", \"poNumber\": \"\", \"glAccount\": null, \"requisition\": \"748720\", \"ioNumberOwner\": \"ernazarova.y@pg.com\", \"finalAssetApprovalDate\": null}" }
            };
        }

        private List<CostFormDetails> SetUpCostFormDetails(Guid costStageRevisionId)
        {
            return new List<CostFormDetails>() {
                new CostFormDetails { Id = Guid.NewGuid(), CostStageRevisionId = costStageRevisionId }
            };
        }

        private List<ExpectedAsset> SetUpExpectedAssets(Guid costStageRevisionId)
        {
            return new List<ExpectedAsset>() {
                new ExpectedAsset { Id = Guid.NewGuid(), CostStageRevisionId = costStageRevisionId, ProjectAdId = new ProjectAdId { } }
            };
        }

        private CustomFormData SetUpStageDetails()
        {
            return new CustomFormData
            {
                Id = Guid.NewGuid(),
                Data = "{\"airing\": \"NotForAir\", \"director\": \"Richard Auyeung\", \"shootDays\": 1, \"firstShootDate\": \"2018-08-02T16:00:00Z\", \"primaryShootCity\": {\"id\": \"5b8edc3b-e5d6-402b-8b5b-4a9762d283e6\", \"name\": \"Shanghai\", \"countryId\": \"42617184-9e39-480b-8ed6-f0b563543d1d\", \"isCapital\": false, \"countryCapital\": null}, \"productionCompany\": {\"id\": \"5515de5e-119e-11e8-905d-0a5d70ceb10e\", \"name\": \"Bleu Arc Pictures Production Ltd\", \"sapVendorCode\": null, \"isDirectPayment\": false, \"hasDirectPayment\": false, \"defaultCurrencyId\": \"c1e3ad8f-26ce-407d-9e00-c1380c3e5e10\", \"productionCategory\": \"ProductionCompany\", \"isPreferredSupplier\": false}, \"primaryShootCountry\": {\"id\": \"42617184-9e39-480b-8ed6-f0b563543d1d\", \"name\": \"China\"}, \"postProductionCompany\": {\"id\": \"24ed368b-90c2-4a5e-b9ef-55598f76815d\", \"name\": \"PING PONG LTD\", \"sapVendorCode\": \"15056270\", \"isDirectPayment\": false, \"hasDirectPayment\": true, \"defaultCurrencyId\": \"7507ec61-0777-4512-8afc-a415180dbbee\", \"productionCategory\": \"PostProductionCompany\", \"isPreferredSupplier\": false}}"
            };
        }

        private CustomFormData SetUpProductDetails()
        {
            return new CustomFormData
            {
                Id = Guid.NewGuid(),
                Data = "{\"smoId\": null, \"title\": \"Downy Divas TVC - making off video\", \"isAIPE\": false, \"campaign\": \"Downy Divas TVC Shooting\", \"costType\": \"Production\", \"projectId\": \"5b3e0468b9fc6658bce6393f\", \"costNumber\": \"PRO0002329V0009\", \"contentType\": {\"id\": \"d64450c1-8a27-4b31-bb5f-1f9240597be9\", \"key\": \"Video\", \"value\": \"Video\", \"created\": \"2018-02-14T15:47:15.121647\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.121646\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"f2a40aba-5066-4a8b-b87a-7e830224dd0f\"}, \"description\": \"Downy Divas TVC - making off video\", \"budgetRegion\": {\"id\": \"80487b2d-f51c-4de2-b0d0-5cbe3aec3d64\", \"key\": \"GREATER CHINA AREA\", \"name\": \"Greater China\"}, \"organisation\": {\"id\": \"232146fa-7b1f-4233-90fd-c92fec802e14\", \"key\": \"BFO\", \"value\": \"BFO\", \"created\": \"2018-02-14T15:47:15.134185\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.134184\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"c81e485e-10ab-423f-9597-0959430d3be6\"}, \"initialBudget\": 3000, \"agencyCurrency\": \"HKD\", \"agencyProducer\": [\"Maggie Ng (Grey HK)\"], \"productionType\": {\"id\": \"38f2c69d-d0d2-48a5-a8e6-96d52383b4fb\", \"key\": \"Full Production\", \"value\": \"Full Production\"}, \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"PNGALENC1800586\"}"
            };
        }


        private CostTemplateVersion SetUpCostTemplateVersion()
        {
            return new CostTemplateVersion
            {
                Id = Guid.NewGuid(),
                Name = RandomString(5),
                ProductionDetailsTemplates = new List<ProductionDetailsTemplate>(),
                FormDefinitions = new List<FormDefinition>(),
                CostTemplate = SetupCostTemplate()
            };
        }


        private CostTemplate SetupCostTemplate()
        {
            return new CostTemplate
            {
                Id = Guid.NewGuid(),
                Name = RandomString(5),
                Label = RandomString(6)
            };
        }


        private List<Approval> SetUpApprovals(CostUser creator, CostUser ipmUser, CostUser brandUser)
        {
            return new List<Approval>() {
                new Approval {
                    Id = Guid.NewGuid(),
                    ApprovalMembers = new List<ApprovalMember>() {
                        new ApprovalMember () {
                            Id = Guid.NewGuid(),
                            CostUser = ipmUser,
                            ApprovalDetails = new ApprovalDetails() {
                                Id = Guid.NewGuid()
                            },
                            Status = ApprovalStatus.Approved
                        },
                        new ApprovalMember () {
                            Id = Guid.NewGuid(),
                            CostUser = brandUser,
                            ApprovalDetails = new ApprovalDetails() {
                                Id = Guid.NewGuid()
                            },
                            Status = ApprovalStatus.Approved
                        }
                    },
                    Requisitioners = new List<Requisitioner> {
                        new Requisitioner {
                            Id = Guid.NewGuid(),
                            CostUser = creator,
                            ApprovalDetails = new ApprovalDetails() {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    Status = ApprovalStatus.Approved
                },
            };
        }

        private void SetupData()
        {
            var creator = new CostUser
            {
                Id = UserIdentity.Id,
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Country = new Country
                    {
                        Id = Guid.NewGuid(),
                        Name = "Afghanistan",
                        Iso = "AF"
                    }
                }
            };
            var ipmUser = new CostUser()
            {
                Id = Guid.NewGuid(),
            };
            var brandUser = new CostUser()
            {
                Id = Guid.NewGuid(),
            };

            var oeRevisionId = Guid.NewGuid();
            var oeRevision = new CostStageRevision()
            {
                Id = oeRevisionId,
                TravelCosts = new List<TravelCost>
                {
                    SetUpTravelCost(),
                },
                CostStageRevisionPaymentTotals = SetUpCostStageRevisionPaymentTotal(),
                CostLineItems = SetUpCostLineItem(oeRevisionId),
                StageDetails = SetUpStageDetails(),
                ProductDetails = SetUpProductDetails(),
                CostFormDetails = SetUpCostFormDetails(oeRevisionId),
                Created = DateTime.Now.AddDays(-2),
            };

            var fpRevisionId = Guid.NewGuid();
            var fpRevision = new CostStageRevision()
            {
                Id = fpRevisionId,
                TravelCosts = new List<TravelCost>
                {
                    SetUpTravelCost(),
                },
                CostStageRevisionPaymentTotals = SetUpCostStageRevisionPaymentTotal(),
                CostLineItems = SetUpCostLineItem(fpRevisionId),
                StageDetails = SetUpStageDetails(),
                ProductDetails = SetUpProductDetails(),
                CostFormDetails = SetUpCostFormDetails(fpRevisionId),
                Created = DateTime.Now.AddDays(-1),
            };

            var faRevision = new CostStageRevision()
            {
                Id = RevisionId,
                TravelCosts = new List<TravelCost>
                {
                    SetUpTravelCost(),
                },
                CostStageRevisionPaymentTotals = SetUpCostStageRevisionPaymentTotal(),
                CostLineItems = SetUpCostLineItem(RevisionId),
                Approvals = SetUpApprovals(creator, ipmUser, brandUser),
                StageDetails = SetUpStageDetails(),
                ProductDetails = SetUpProductDetails(),
                CostFormDetails = SetUpCostFormDetails(RevisionId),
                Created = DateTime.Now,
            };

            var costTemplateVersion = SetUpCostTemplateVersion();
            var cost = new Cost()
            {
                Id = CostId,
                Deleted = false,
                PaymentCurrency = new Currency
                {
                    Code = "USD",
                    DefaultCurrency = true,
                    Id = CurrencyUsdId,
                },
                Project = new Project
                {
                    Id = Guid.NewGuid()
                },
                CreatedBy = creator,
                Owner = creator,
                NotificationSubscribers = new List<NotificationSubscriber>() {
                    new NotificationSubscriber() { Id = Guid.NewGuid(), CostUser = ipmUser },
                    new NotificationSubscriber() { Id = Guid.NewGuid(), CostUser = brandUser }
                },
                CostStages = new List<CostStage>()
                {
                    new CostStage() { Id = Guid.NewGuid(), CostStageRevisions = new List<CostStageRevision>() { oeRevision } },
                    new CostStage() { Id = Guid.NewGuid(), CostStageRevisions = new List<CostStageRevision>() { fpRevision } },
                    new CostStage() { Id = Guid.NewGuid(), CostStageRevisions = new List<CostStageRevision>() { faRevision } }
                },
                CreatedById = creator.Id,
                OwnerId = creator.Id,
                CostTemplateVersion = costTemplateVersion,
                CostTemplateVersionId = costTemplateVersion.Id
            };

            EFContext.Cost.Add(cost);
            EFContext.SaveChanges();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

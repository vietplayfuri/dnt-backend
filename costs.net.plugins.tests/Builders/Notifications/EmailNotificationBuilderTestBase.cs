
namespace costs.net.plugins.tests.Builders.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Helpers;
    using core.Mapping;
    using core.Models.Notifications;
    using core.Models.Regions;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.CustomData;
    using core.Services.Notifications;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Builders.Notifications;
    using plugins.PG.Form;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;
    using Cost = dataAccess.Entity.Cost;
    using Project = dataAccess.Entity.Project;

    public abstract class EmailNotificationBuilderTestBase
    {
        private readonly string _costUrl;

        private EmailNotificationBuilder _emailNotificationBuilder;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private Mock<IApprovalService> _approvalServiceMock;
        private Mock<IRegionsService> _regionsServiceMock;
        private Mock<ICostFormService> _costFormServiceMock;
        private Mock<ICustomObjectDataService> _customObjectDataServiceMock;
        private Mock<IPgPaymentService> _pgPaymentServiceMock;
        private Mock<ICostStageService> _costStageServiceMock;
        private AppSettings _appSettings;
        protected EFContext EFContext;

        private readonly Guid _northAmericanRegionId = Guid.NewGuid();
        private readonly Guid _europeRegionId = Guid.NewGuid();
        private readonly Guid _asiaRegionId = Guid.NewGuid();

        protected string CostUrl => _costUrl;
        protected EmailNotificationBuilder EmailNotificationBuilder => _emailNotificationBuilder;
        protected Mock<ICostStageRevisionService> CostStageRevisionServiceMock => _costStageRevisionServiceMock;
        protected Mock<IApprovalService> ApprovalServiceMock => _approvalServiceMock;
        protected Mock<IRegionsService> RegionsServiceMock => _regionsServiceMock;
        protected Mock<ICostFormService> CostFormServiceMock => _costFormServiceMock;
        protected Mock<ICustomObjectDataService> CustomObjectDataServiceMock => _customObjectDataServiceMock;
        protected Mock<IPgPaymentService> PgPaymentServiceMock => _pgPaymentServiceMock;
        protected Mock<ICostStageService> CostStageServiceMock => _costStageServiceMock;
        protected Mock<IMetadataProviderService> MetadataProviderServiceMock;
        protected ICollection<MetadataItem> MetadataItems;
        protected AppSettings AppSettings => _appSettings;

        protected Guid NorthAmericanRegionId => _northAmericanRegionId;
        protected Guid EuropeRegionId => _europeRegionId;
        protected Guid AsiaRegionId => _asiaRegionId;

        protected const string CostNumber = "P101";
        protected const CostStages CostStageName = CostStages.Aipe;
        protected const string AgencyTrackingNumber = "Cost agency tracking number";
        protected const string CostOwnerFullName = "Mr Cost Owner";
        protected const string ProjectName = "Pampers";
        protected const string CostProductionType = Constants.ProductionType.PostProductionOnly;
        protected const string ContentType = Constants.ContentType.Photography;
        protected const string ProjectGdamId = "57e5461ed9563f268ef4f19c";
        protected const string ProjectNumber = "PandG01";
        protected const string AgencyLocation = "United Kingdom";
        protected const string AgencyName = "Saatchi";
        protected const string BrandName = "P&G";
        protected const string CostOwnerGdamUserId = "57e5461ed9563f268ef4f19d";
        protected const string CostTitle = "My Cost Title";
        protected const string BrandManagerRoleValue = "Brand Management Approver";

        protected EmailNotificationBuilderTestBase()
        {
            _costUrl ="https://www.google.co.uk";
        }

        [SetUp]
        public void Init()
        {
            EFContext = EFContextFactory.CreateInMemoryEFContext();
            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(NotificationProfile),
                    typeof(PgPurchaseOrderResponse)
                )
            );

            IMapper mapper = new Mapper(configuration);
            var mockUriHelper = new Mock<IApplicationUriHelper>();

            _appSettings = new AppSettings();
            var appSettingsMock = new Mock<IOptions<AppSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_appSettings);

            mockUriHelper.Setup(u => u.GetLink(It.IsAny<ApplicationUriName>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_costUrl);

            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _approvalServiceMock = new Mock<IApprovalService>();

            _regionsServiceMock = new Mock<IRegionsService>();

            _regionsServiceMock.Setup(r => r.GetAsync(_northAmericanRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _northAmericanRegionId,
                Name = "North America"
            }));
            _regionsServiceMock.Setup(r => r.GetAsync(_europeRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _europeRegionId,
                Name = "Europe"
            }));
            _regionsServiceMock.Setup(r => r.GetAsync(_asiaRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _asiaRegionId,
                Name = "Asia"
            }));
            _regionsServiceMock.Setup(r => r.GetGeoRegion(_northAmericanRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _northAmericanRegionId,
                Name = "North America"
            }));
            _regionsServiceMock.Setup(r => r.GetGeoRegion(_europeRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _europeRegionId,
                Name = "Europe"
            }));
            _regionsServiceMock.Setup(r => r.GetGeoRegion(_asiaRegionId)).Returns(Task.FromResult(new RegionModel
            {
                Id = _asiaRegionId,
                Name = "Asia"
            }));

            _costFormServiceMock = new Mock<ICostFormService>();
            _customObjectDataServiceMock = new Mock<ICustomObjectDataService>();
            _pgPaymentServiceMock = new Mock<IPgPaymentService>();
            _costStageServiceMock = new Mock<ICostStageService>();
            MetadataItems = new List<MetadataItem>();
            MetadataProviderServiceMock = new Mock<IMetadataProviderService>();
            MetadataProviderServiceMock.Setup(m => m.Provide(It.IsAny<Guid>())).Returns(Task.FromResult(MetadataItems));

            const string brandManagerRoleKey = "Brand Manager";
            var brandManagerRole = new BusinessRole
            {
                Key = brandManagerRoleKey,
                Value = BrandManagerRoleValue
            };
            EFContext.BusinessRole.Add(brandManagerRole);
            EFContext.SaveChanges();

            _emailNotificationBuilder = new EmailNotificationBuilder(mapper,
                mockUriHelper.Object, 
                _costStageRevisionServiceMock.Object, 
                _approvalServiceMock.Object, 
                _regionsServiceMock.Object,
                appSettingsMock.Object, 
                _costFormServiceMock.Object, 
                _customObjectDataServiceMock.Object,
                _pgPaymentServiceMock.Object,
                _costStageServiceMock.Object,
                MetadataProviderServiceMock.Object,
                EFContext);
        }

        protected void SetupDataSharedAcrossTests(Agency agency, Country country, 
            Cost cost, CostStageRevision latestRevision, Project project, CostUser costOwner, Guid costOwnerId, CostStage costStage,
            Brand brand, Guid costId, Guid costStageRevisionId, Guid projectId, string budgetRegion = Constants.BudgetRegion.AsiaPacific)
        {
            agency.Country = country;
            cost.CostNumber = CostNumber;
            cost.LatestCostStageRevision = latestRevision;
            cost.Project = project;
            costOwner.Agency = agency;
            costOwner.Id = costOwnerId;
            latestRevision.CostStage = costStage;
            project.Brand = brand;

            agency.Name = AgencyName;
            brand.Name = BrandName;
            cost.Id = costId;
            costStage.Name = CostStageName.ToString();
            costOwner.FullName = CostOwnerFullName;
            costOwner.GdamUserId = CostOwnerGdamUserId;
            latestRevision.Id = costStageRevisionId;
            project.Id = projectId;
            project.Name = ProjectName;
            project.GdamProjectId = ProjectGdamId;
            project.AdCostNumber = ProjectNumber;
            country.Name = AgencyLocation;

            var stageDetails = new PgStageDetailsForm
            {
                ContentType = new core.Builders.DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = ContentType
                },
                CostType = cost.CostType.ToString(),
                ProductionType = new core.Builders.DictionaryValue
                {
                    Id = Guid.NewGuid(),
                    Key = CostProductionType
                },
                Title = CostTitle,
                AgencyTrackingNumber = AgencyTrackingNumber,
                BudgetRegion = new AbstractTypeValue
                {
                    Key = budgetRegion,
                    Name = budgetRegion
                }
            };
            var existingUser = EFContext.CostUser.FirstOrDefault(a => a.GdamUserId == CostOwnerGdamUserId);
            if (existingUser == null)
            {
                EFContext.Add(costOwner);
                EFContext.SaveChanges();
            }

            CostStageRevisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(costStageRevisionId)).ReturnsAsync(stageDetails);
        }

        protected void TestCommonMessageDetails(EmailNotificationMessage<CostNotificationObject> emailNotificationMessage, string expectedActionType, params string[] emailRecipients)
        {
            emailNotificationMessage.Should().NotBeNull();
            emailNotificationMessage.Action.Should().NotBeNull();
            emailNotificationMessage.Action.Type.Should().NotBeNull();
            emailNotificationMessage.Object.Should().NotBeNull();
            emailNotificationMessage.Object.Agency.Should().NotBeNull();
            emailNotificationMessage.Object.Brand.Should().NotBeNull();
            emailNotificationMessage.Object.Cost.Should().NotBeNull();
            emailNotificationMessage.Object.Parents.Should().NotBeNull();
            emailNotificationMessage.Object.Type.Should().NotBeNull();

            emailNotificationMessage.Recipients.Should().NotBeNull();
            emailNotificationMessage.Subject.Should().NotBeNull();
            emailNotificationMessage.Viewers.Should().NotBeNull();

            emailNotificationMessage.Action.Type.Should().Be(expectedActionType);
            emailNotificationMessage.Type.Should().NotBeNull();
            emailNotificationMessage.Type.Should().Be(expectedActionType); //Paper-Pusher uses .Type, MS uses Action.Type. Both should be same.
            emailNotificationMessage.Object.Type.Length.Should().BeGreaterThan(0); //Required by Paper-Pusher

            emailNotificationMessage.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow); //Not a future notification

            //Identifiers are set to something other than Null/Empty
            emailNotificationMessage.Id.Should().NotBe(Guid.Empty);
            emailNotificationMessage.Object.Id.Should().NotBe(Guid.Empty.ToString());
            emailNotificationMessage.Subject.Id.Should().NotBe(Guid.Empty.ToString());

            emailNotificationMessage.Object.Agency.Location.Should().NotBeNull();
            emailNotificationMessage.Object.Agency.Location.Should().Be(AgencyLocation);
            emailNotificationMessage.Object.Agency.Name.Should().NotBeNull();
            emailNotificationMessage.Object.Agency.Name.Should().Be(AgencyName);

            emailNotificationMessage.Object.Brand.Name.Should().NotBeNull();
            emailNotificationMessage.Object.Brand.Name.Should().Be(BrandName);

            emailNotificationMessage.Object.Cost.Number.Should().Be(CostNumber);
            emailNotificationMessage.Object.Cost.Title.Should().NotBeNull();
            //TODO: Double check this is correct field to use
            emailNotificationMessage.Object.Cost.Title.Should().Be(CostTitle);
            emailNotificationMessage.Object.Cost.ProductionType.Should().NotBeNull();
            emailNotificationMessage.Object.Cost.ProductionType.Should().Be(CostProductionType);
            emailNotificationMessage.Object.Cost.ContentType.Should().NotBeNull();
            emailNotificationMessage.Object.Cost.ContentType.Should().Be(ContentType);
            emailNotificationMessage.Object.Cost.Owner.Should().NotBeNull();
            emailNotificationMessage.Object.Cost.Owner.Should().Be(CostOwnerFullName);
            emailNotificationMessage.Object.Cost.Url.Should().NotBeNull();
            emailNotificationMessage.Object.Cost.Url.Should().Be(CostUrl);

            emailNotificationMessage.Object.Project.Id.Should().Be(ProjectGdamId);
            emailNotificationMessage.Object.Project.Name.Should().NotBeNull();
            emailNotificationMessage.Object.Project.Name.Should().Be(ProjectName);
            emailNotificationMessage.Object.Project.Number.Should().Be(ProjectNumber);


            emailNotificationMessage.Recipients.Should().Contain(emailRecipients);

            emailNotificationMessage.Action.Share.Should().NotBeNull();
            emailNotificationMessage.Action.Share.To.Count.Should().BeGreaterOrEqualTo(1);
        }
    }
}


namespace costs.net.plugins.tests.Services.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services.Notifications;
    using Brand = dataAccess.Entity.Brand;
    using Cost = dataAccess.Entity.Cost;

    [TestFixture]
    public class MetadataProviderServiceTests
    {
        private Mock<EFContext> _efContextMock;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private PgStageDetailsForm _stageDetails;
        private MetadataProviderService _target;

        private Cost _cost;

        private readonly Guid _costId = Guid.NewGuid();
        private readonly Guid _costOwnerId = Guid.NewGuid();
        private readonly Guid _insuranceUserId = Guid.NewGuid();
        private readonly Guid _approverUserId = Guid.NewGuid();
        private readonly Guid _costStageRevisionId = Guid.NewGuid();

        [SetUp]
        public void Init()
        {
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _efContextMock = new Mock<EFContext>();

            SetupDataSharedAcrossTests();

            _target = new MetadataProviderService(_efContextMock.Object, _costStageRevisionServiceMock.Object);
        }

        [Test]
        public async Task Provide_EmptyCostId_ReturnsEmptyCollection()
        {
            var costId = Guid.Empty;

            var result = await _target.Provide(costId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Provide_CostIdDoesNotExist_ReturnsEmptyCollection()
        {
            var costId = Guid.NewGuid();

            var result = await _target.Provide(costId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Provide_NoStageDetails_ReturnsEmptyCollection()
        {
            var costId = Guid.NewGuid();

            var result = await _target.Provide(costId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Provide_CostIdDoesExist_ReturnsMetadata()
        {
            var costId = _costId;
            var expectedCount = 11;

            var result = await _target.Provide(costId);

            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }
        
        private void SetupDataSharedAcrossTests()
        {
            const string agencyLocation = "United Kingdom";
            const string agencyName = "Saatchi";
            const string brandName = "P&G";
            const string costNumber = "P101";
            const CostStages costStageName = CostStages.OriginalEstimate;
            const string costOwnerGdamUserId = "57e5461ed9563f268ef4f19d";
            const string costOwnerFullName = "Mr Cost Owner";
            const string projectName = "Pampers";
            const string projectGdamId = "57e5461ed9563f268ef4f19c";
            const string projectNumber = "PandG01";

            var projectId = Guid.NewGuid();

            _cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.FinanceManager,
                            Value = Constants.BusinessRole.FinanceManager
                        }
                    }
                }
            };
            var approverUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.Ipm,
                            Value = Constants.BusinessRole.Ipm
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            agency.Country = country;
            approverUser.Id = _approverUserId;
            _cost.CreatedBy = costOwner;
            _cost.CreatedById = _costOwnerId;
            _cost.Owner = costOwner;
            _cost.OwnerId = _costOwnerId;
            _cost.CostNumber = costNumber;
            _cost.LatestCostStageRevision = latestRevision;
            _cost.Project = project;
            costOwner.Agency = agency;
            costOwner.Id = _costOwnerId;
            insuranceUser.Id = _insuranceUserId;
            latestRevision.CostStage = costStage;
            project.Brand = brand;

            agency.Name = agencyName;
            brand.Name = brandName;
            _cost.Id = _costId;
            costStage.Name = costStageName.ToString();
            costOwner.FullName = costOwnerFullName;
            costOwner.GdamUserId = costOwnerGdamUserId;
            costOwner.Id = _costOwnerId;
            latestRevision.Id = _costStageRevisionId;
            project.Id = projectId;
            project.Name = projectName;
            project.GdamProjectId = projectGdamId;
            project.AdCostNumber = projectNumber;
            country.Name = agencyLocation;

            var agencies = new List<Agency> { agency };
            var brands = new List<Brand> { brand };
            var costs = new List<Cost> { _cost };
            var costStages = new List<CostStageRevision> { latestRevision };
            var costUsers = new List<CostUser> { approverUser, costOwner, insuranceUser };
            var countries = new List<Country> { country };
            var projects = new List<Project> { project };

            _efContextMock.MockAsyncQueryable(agencies.AsQueryable(), c => c.Agency);
            _efContextMock.MockAsyncQueryable(brands.AsQueryable(), c => c.Brand);
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), c => c.Cost);
            _efContextMock.MockAsyncQueryable(costStages.AsQueryable(), c => c.CostStageRevision);
            _efContextMock.MockAsyncQueryable(costUsers.AsQueryable(), c => c.CostUser);
            _efContextMock.MockAsyncQueryable(countries.AsQueryable(), c => c.Country);
            _efContextMock.MockAsyncQueryable(projects.AsQueryable(), c => c.Project);

            _efContextMock.MockAsyncQueryable(new List<NotificationSubscriber>
            {
                new NotificationSubscriber
                {
                    CostId = _cost.Id,
                    CostUserId = _costOwnerId,
                    CostUser = costOwner
                }
            }.AsQueryable(), a => a.NotificationSubscriber);

            _stageDetails = new PgStageDetailsForm
            {
                Title = "Test Title",
                BudgetRegion = new AbstractTypeValue
                {
                    Name = Constants.BudgetRegion.AsiaPacific
                },
                ContentType = new core.Builders.DictionaryValue
                {
                    Value = Constants.ContentType.Audio
                },
                ProductionType = new core.Builders.DictionaryValue
                {
                    Value = Constants.ProductionType.PostProductionOnly
                }
            };
            _costStageRevisionServiceMock.Setup(c => c.GetStageDetails<PgStageDetailsForm>(_cost.LatestCostStageRevision)).Returns(_stageDetails);
        }
    }
}

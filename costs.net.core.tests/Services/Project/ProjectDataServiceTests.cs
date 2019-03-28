namespace costs.net.core.tests.Services.Project
{
    using System;
    using System.Threading.Tasks;
    using Builders.Response;
    using core.Models.Costs;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services.Project;
    using core.Services.Search;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Exception;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class ProjectDataServiceTests
    {
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private EFContext _efContext;
        private ProjectDataService _projectDataService;
        private Mock<IOptions<AppSettings>> _optionsMock;

        [SetUp]
        public void Init()
        {
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _optionsMock = new Mock<IOptions<AppSettings>>();
            _optionsMock.Setup(o => o.Value).Returns(new AppSettings());
            _efContext.Currency.Add(new Currency
            {
                DefaultCurrency = true,
                Symbol = "$",
                Code = "USD"
            });
            _efContext.SaveChanges();

            _projectDataService = new ProjectDataService(_elasticSearchServiceMock.Object, _efContext, _optionsMock.Object);
        }

        [Test]
        public void GetProjectTotals_WhenIdentityIsNull_ShouldThrowException()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            // Act
            // Assert
            _projectDataService.Awaiting(r => r.GetProjectTotals(projectId, null)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void GetProjectTotals_WhenProjectNotDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var project = Guid.Empty;
            var userIdentity = new UserIdentity();

            // Act
            // Assert
            _projectDataService.Awaiting(r => r.GetProjectTotals(project, userIdentity)).ShouldThrow<EntityNotFoundException<Project>>();
        }

        [Test]
        public async Task GetProjectTotals_WhenInputIsValid_ShouldSearchCostsByProjectIdAndUserId()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userIdentity = new UserIdentity { Id = userId };
            _efContext.Project.Add(new Project
            {
                Id = projectId,
                CreatedBy = new CostUser
                {
                    Agency = new Agency()
                },
                Brand = new Brand(),
            });
            _efContext.CostUser.Add(new CostUser
            {
                Id = userId,
                Agency = new Agency()
            });
            _efContext.SaveChanges();

            // Act
            await _projectDataService.GetProjectTotals(projectId, userIdentity);

            // Assert
            _elasticSearchServiceMock.Verify(es => 
                es.SearchCosts(It.Is<CostQuery>(q => q.ProjectId == projectId.ToString()), userId), Times.Once);
        }

        [Test]
        public async Task GetProjectTotals_WhenSearchReturnsSingleCost_ShouldReturnSingleItemInTotals()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userIdentity = new UserIdentity { Id = userId };
            _elasticSearchServiceMock.Setup(es => es.SearchCosts(It.IsAny<CostQuery>(), It.IsAny<Guid>()))
                .ReturnsAsync((new[] { new CostSearchItem() }, 1));
            _efContext.CostUser.Add(new CostUser
            {
                Id = userId,
                Agency = new Agency()
            });
            _efContext.Project.Add(new Project
            {
                Id = projectId,
                CreatedBy = new CostUser
                {
                    Agency = new Agency()
                },
                Brand = new Brand(),
            });
            _efContext.SaveChanges();

            // Act
            var result = await _projectDataService.GetProjectTotals(projectId, userIdentity);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
        }

        [Test]
        public async Task GetProjectTotals_WhenSearchReturnsSingleCost_ShouldPopulateEachFieldCorrectly()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string userFullName = "User Business Unit 1";
            const string userBusinessUnit = "User Business Unit 1";

            var userIdentity = new UserIdentity { Id = userId, FullName = userFullName };
            const string budgetRegion = "Test Budget Region";
            const string budgetRegionName = "Test Budget Region Name";
            const string costNumber = "Cost #";
            const string costStatus = "Draft";
            const string stageName = "Original Estimate";
            const string contentType = "Video";
            const string contentTypeValue = "Video Name";
            const string productionType = "Full Production";
            const string productionTypeValue = "Full Production Value";
            DateTime costCreated = DateTime.UtcNow.AddDays(-1);
            const string costTitle = "Test Cost 1";
            const decimal budget = 123456;
            const string costOwnerEmail = "Cost Owner Email";
            const decimal totalLocalCurrency = 234667;
            const decimal totalDefaultCurrency = 100;
            const string currencySymbol = "$";
            const string projectCostNumber = "PRO12345";
            const string projectName = "Project Name 1";
            DateTime projectCreated = DateTime.UtcNow.AddDays(-2);
            const string projectCreatorEmail = "Project Creator Email 1";
            const string projectCreatorAgency = "Project Creator Agency 1";
            const string brand = "Brand 1";
            const string sector = "Sector 1";

            var costSeacrhItem = new CostSearchItem
            {
                BudgetRegion = budgetRegion,
                BudgetRegionName = budgetRegionName,
                CostNumber = costNumber,
                Status = costStatus,
                Stage = stageName,
                ContentType = contentType,
                ContentTypeValue = contentTypeValue,
                ProductionType = productionType,
                ProductionTypeValue = productionTypeValue,
                CreatedDate = costCreated,
                Title = costTitle,
                Budget = budget,
                CostOwner = costOwnerEmail,
                GrandTotal = totalLocalCurrency,
                GrandTotalDefaultCurrency = totalDefaultCurrency,
                CurrencySymbol = currencySymbol
            };
            _elasticSearchServiceMock.Setup(es => es.SearchCosts(It.IsAny<CostQuery>(), It.IsAny<Guid>()))
                .ReturnsAsync((new[] { costSeacrhItem }, 1));

            _efContext.CostUser.Add(new CostUser
            {
                Id = userId,
                Agency = new Agency
                {
                    Name = userBusinessUnit
                },
                FullName = userFullName
            });
            _efContext.Project.Add(new Project
            {
                Id = projectId,
                AdCostNumber = projectCostNumber,
                Name = projectName,
                Created = projectCreated,
                CreatedBy = new CostUser
                {
                    Email = projectCreatorEmail,
                    Agency = new Agency
                    {
                        Name = projectCreatorAgency
                    }
                },
                Brand = new Brand
                {
                    Name = brand,
                    Sector = new Sector
                    {
                        Name = sector
                    }
                },
            });
            _efContext.SaveChanges();

            // Act
            var result = await _projectDataService.GetProjectTotals(projectId, userIdentity);

            // Assert
            result.Should().NotBeNull();
            result.Summary.ProjectCurrentCostsTotal.Should().Be($"{currencySymbol}{totalDefaultCurrency:N3}");
            var item = result.Items[0];

            item.BudgetRegion.Should().Be(budgetRegionName);
            item.CostNumber.Should().Be(costNumber);
            item.CostStatus.Should().Be(costStatus);
            item.CostStageName.Should().Be(stageName);
            item.ContentType.Should().Be(contentTypeValue);
            item.ProductionType.Should().Be(productionTypeValue);
            item.CostCreated.Should().Be(costCreated.ToString("dd/MM/yyyy"));
            item.CostTitle.Should().Be(costTitle);
            item.TargetBudget.Should().Be($"{currencySymbol}{budget:N3}");
            item.CostOwnerEmail.Should().Be(costOwnerEmail);
            item.CostCurrentTotalLocalCurrency.Should().Be($"{currencySymbol}{totalLocalCurrency:N3}");
            item.CostCurrentTotalDefaulCurrency.Should().Be($"{currencySymbol}{totalDefaultCurrency:N3}");

            // Project Info 
            result.Summary.ProjectId.Should().Be(projectCostNumber);
            result.Summary.ProjectName.Should().Be(projectName);
            result.Summary.ProjectCreationDate.Should().Be(projectCreated.ToString("dd/MM/yyyy"));
            result.Summary.ProjectCreatorEmail.Should().Be(projectCreatorEmail);
            result.Summary.ProjectCreatorAgency.Should().Be(projectCreatorAgency);
            result.Summary.Brand.Should().Be(brand);
            result.Summary.Sector.Should().Be(sector);

            // User Info
            result.Summary.UserBusinessUnit.Should().Be(userBusinessUnit);
            result.Summary.UserFullName.Should().Be(userFullName);
        }
    }
}

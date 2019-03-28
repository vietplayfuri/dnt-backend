namespace costs.net.plugins.tests.PG.Services.Role
{
    using System;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services.Role;

    [TestFixture]
    public class PgCostUserRoleServiceTests
    {
        private EFContext _efContext;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private PgCostUserRoleService _pgCostUserRoleService;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();

            _pgCostUserRoleService = new PgCostUserRoleService(_efContext, _costStageRevisionServiceMock.Object);
        }

        [Test]
        public async Task DoesUserHaveRoleForCost_When_UserHasBusinessRoleOnClientModule_Should_ReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            const string businessRole = Constants.BusinessRole.FinanceManager;
            var stageDetails = new PgStageDetailsForm();

            var cost = new Cost
            {
                Id = costId,
                LatestCostStageRevision = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    StageDetails = new CustomFormData
                    {
                        Data = JsonConvert.SerializeObject(stageDetails)
                    }
                }
            };
            _costStageRevisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r == cost.LatestCostStageRevision)))
                .Returns(stageDetails);

            _efContext.Cost.Add(cost);
            _efContext.UserBusinessRole.Add(new UserBusinessRole
            {
                CostUser = new CostUser
                {
                    Id = userId
                },
                BusinessRole = new BusinessRole
                {
                    Key = businessRole
                },
                ObjectId = Guid.NewGuid()
            });
            _efContext.SaveChanges();

            // Act
            var result = await _pgCostUserRoleService.DoesUserHaveRoleForCost(userId, costId, businessRole);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task DoesUserHaveRoleForCost_When_UserHasBusinessRoleAgainstOnSmoAndSmoMatches_Should_ReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            const string smoName = "test SMO";
            const string businessRole = Constants.BusinessRole.FinanceManager;
            var stageDetails = new PgStageDetailsForm
            {
                SmoName = smoName
            };

            var cost = new Cost
            {
                Id = costId,
                LatestCostStageRevision = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    StageDetails = new CustomFormData
                    {
                        Data = JsonConvert.SerializeObject(stageDetails)
                    }
                }
            };
            _costStageRevisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r == cost.LatestCostStageRevision)))
                .Returns(stageDetails);

            _efContext.Cost.Add(cost);
            _efContext.UserBusinessRole.Add(new UserBusinessRole
            {
                CostUser = new CostUser
                {
                    Id = userId
                },
                BusinessRole = new BusinessRole
                {
                    Key = businessRole
                },
                Labels = new []{ smoName }
            });
            _efContext.SaveChanges();

            // Act
            var result = await _pgCostUserRoleService.DoesUserHaveRoleForCost(userId, costId, businessRole);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task DoesUserHaveRoleForCost_When_UserHasBusinessRoleAgainstOnSmoAndSmoDoesNotMatch_Should_ReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            const string smoStageDetails = "stage details SMO";
            const string smoAccessRule = "access rule SMO";
            const string businessRole = Constants.BusinessRole.FinanceManager;
            var stageDetails = new PgStageDetailsForm
            {
                SmoName = smoStageDetails
            };

            var cost = new Cost
            {
                Id = costId,
                LatestCostStageRevision = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    StageDetails = new CustomFormData
                    {
                        Data = JsonConvert.SerializeObject(stageDetails)
                    }
                }
            };
            _costStageRevisionServiceMock.Setup(csr => csr.GetStageDetails<PgStageDetailsForm>(It.Is<CostStageRevision>(r => r == cost.LatestCostStageRevision)))
                .Returns(stageDetails);

            _efContext.Cost.Add(cost);
            _efContext.UserBusinessRole.Add(new UserBusinessRole
            {
                CostUser = new CostUser
                {
                    Id = userId
                },
                BusinessRole = new BusinessRole
                {
                    Key = businessRole
                },
                Labels = new[] { smoAccessRule }
            });
            _efContext.SaveChanges();

            // Act
            var result = await _pgCostUserRoleService.DoesUserHaveRoleForCost(userId, costId, businessRole);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DoesUserHaveRoleForCost_When_CostDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            const string businessRole = Constants.BusinessRole.FinanceManager;

            _efContext.UserBusinessRole.Add(new UserBusinessRole
            {
                CostUser = new CostUser
                {
                    Id = userId
                },
                BusinessRole = new BusinessRole
                {
                    Key = businessRole
                }
            });
            _efContext.SaveChanges();

            // Act
            var result = await _pgCostUserRoleService.DoesUserHaveRoleForCost(userId, costId, businessRole);

            // Assert
            result.Should().BeFalse();
        }
    }
}
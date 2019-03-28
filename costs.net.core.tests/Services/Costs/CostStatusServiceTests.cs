namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Builders;
    using Builders.Workflow;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Options;
    using core.Models;
    using core.Models.User;
    using core.Models.Utils;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    public class CostStatusServiceTests
    {
        public abstract class CostStatusServiceTest
        {
            private Mock<IOptions<AppSettings>> _appSettingsMock;
            private Mock<DbSet<CostUser>> _costUserMock;
            private Mock<IEventService> _eventServiceMock;
            private List<Lazy<ICostStatusResolver, PluginMetadata>> _costStatusResolvers;

            protected Mock<EFContext> EFContextMock;
            protected Mock<DatabaseFacade> DatabaseFacadeMock;
            protected Mock<IDbContextTransaction> DbContextTransactionMock;
            protected Mock<ICostStatusResolver> CostStatusResolver;
            protected UserIdentity User;
            protected CostStatusService CostStatusService;
            [SetUp]
            public void Setup()
            {
                EFContextMock = new Mock<EFContext>();
                DatabaseFacadeMock = new Mock<DatabaseFacade>(EFContextMock.Object);
                DbContextTransactionMock = new Mock<IDbContextTransaction>();

                DatabaseFacadeMock.Setup(d => d.CurrentTransaction).Returns(DbContextTransactionMock.Object);
                EFContextMock.Setup(ef => ef.Database).Returns(DatabaseFacadeMock.Object);
                EFContextMock.Setup(ef => ef.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                _appSettingsMock = new Mock<IOptions<AppSettings>>();
                _eventServiceMock = new Mock<IEventService>();
                _appSettingsMock.SetupGet(e => e.Value).Returns(new AppSettings());

                User = new UserIdentity
                {
                    Email = "e@mail.com",
                    AgencyId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    BuType = BuType.Pg
                };

                CostStatusResolver = new Mock<ICostStatusResolver>();
                _costStatusResolvers = new List<Lazy<ICostStatusResolver, PluginMetadata>>
                {
                    new Lazy<ICostStatusResolver, PluginMetadata>(() => CostStatusResolver.Object,
                    new PluginMetadata { BuType = User.BuType })
                };

                var costUser = new CostUser
                {
                    Id = User.Id,
                    Email = User.Email,
                    ParentId = User.AgencyId
                };
                _costUserMock = EFContextMock.MockAsyncQueryable(new List<CostUser> { costUser }.AsQueryable(), d => d.CostUser);
                _costUserMock.Setup(cu => cu.FindAsync(User.Id)).ReturnsAsync(costUser);

                CostStatusService = new CostStatusService(
                    EFContextMock.Object,
                    _eventServiceMock.Object,
                    _costStatusResolvers
                );
            }

            protected Cost MockCost()
            {
                var latestRevision = new CostStageRevision
                {
                    Status = CostStageRevisionStatus.PendingReopen,
                    Id = Guid.NewGuid(),
                    CostStage = new CostStage
                    {
                        CostStageRevisions = new List<CostStageRevision>
                        {
                            new CostStageRevision
                            {
                                Status = CostStageRevisionStatus.Approved,
                                Id =Guid.NewGuid(),
                            },
                            new CostStageRevision
                            {
                                Status = CostStageRevisionStatus.PendingReopen,
                                Id =Guid.NewGuid(),
                            },
                        }
                    }
                };
                var cost = new Cost
                {
                    Id = Guid.NewGuid(),
                    LatestCostStageRevisionId = latestRevision.Id,
                    LatestCostStageRevision =  latestRevision,
                };
               
                EFContextMock.MockAsyncQueryable(new List<Cost> { cost }.AsQueryable(), d => d.Cost);
                return cost;
            }
        }

        [TestFixture]
        public class UpdateCostStageRevisionStatusShould : CostStatusServiceTest
        {
            public class TestParam
            {
                public CostStageRevisionStatus Status { get; set; }
                public CostStageRevisionStatus ExpectedCostStatus { get; set; }
                public CostStageRevisionStatus ExpectedLatestRevisionStatus { get; set; }
                public CostStageRevisionStatus OldLatestRevisionStatus { get; set; }
            }

            private static readonly List<TestParam> _testParam = new List<TestParam>
            {
                new TestParam
                {
                    Status = CostStageRevisionStatus.Approved,
                    ExpectedCostStatus = CostStageRevisionStatus.Approved,
                    OldLatestRevisionStatus = CostStageRevisionStatus.Approved,
                    ExpectedLatestRevisionStatus = CostStageRevisionStatus.Approved
                },
                new TestParam
                {
                    Status = CostStageRevisionStatus.ReopenRejected,
                    ExpectedCostStatus = CostStageRevisionStatus.Approved,
                    OldLatestRevisionStatus = CostStageRevisionStatus.ReopenRejected,
                    ExpectedLatestRevisionStatus = CostStageRevisionStatus.Approved
                },
            };
            [Test]
            [TestCaseSource(nameof(_testParam))]
            public async Task UpdateCostStatus(TestParam param)
            {
                // Arrange
                var cost = MockCost();
                var oldLatestRevision = cost.LatestCostStageRevision;

                // Act
                var response = await CostStatusService.UpdateCostStageRevisionStatus(cost.Id, param.Status, BuType.Pg);

                // Assert 
                response.Should().BeTrue();
                cost.Status.Should().Be(param.ExpectedCostStatus);
                cost.LatestCostStageRevision.Status.Should().Be(param.ExpectedLatestRevisionStatus);
                oldLatestRevision.Status.ShouldBeEquivalentTo(param.OldLatestRevisionStatus);
            }
        }
    }
}
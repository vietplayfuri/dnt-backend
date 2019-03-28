using System;
using System.Collections.Generic;
using System.Text;

namespace costs.net.integration.tests.Costs
{
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Builders.Workflow;
    using core.Models;
    using core.Models.Workflow;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class CostStatusServiceTests
    {
        private CostStatusService _costStatusService;
        private readonly EFContext _efContext = EFContextFactory.CreateInMemoryEFContext();
        [SetUp]
        public void Setup()
        {

            var costStatusResolverMock = new Mock<ICostStatusResolver>();

            IEnumerable<Lazy<ICostStatusResolver, PluginMetadata>> metadata = new List<Lazy<ICostStatusResolver, PluginMetadata>>
            {
                new Lazy<ICostStatusResolver, PluginMetadata>(() => costStatusResolverMock.Object,new PluginMetadata{BuType = BuType.Pg})
            };
            _costStatusService = new CostStatusService(_efContext, new Mock<IEventService>().Object, metadata);

            costStatusResolverMock.Setup(s => s.GetNextStatus(It.IsAny<Guid>(), It.IsAny<CostAction>())).ReturnsAsync(CostStageRevisionStatus.Recalled);
        }

        void SetupCost(Guid costId)
        {
            _efContext.Cost.Add(new Cost
            {
                Id = costId,
                LatestCostStageRevision = new CostStageRevision(),
                LatestCostStageRevisionId = Guid.NewGuid()
            });
            _efContext.SaveChanges();
        }

        [Test]
        public async Task UpdateCostStatus_Should_Return_Status()
        {
            var costId = Guid.NewGuid();
            SetupCost(costId);
            var response = await _costStatusService.UpdateCostStatus(BuType.Pg, costId, CostAction.Recall);

            response.Messages.Count().ShouldBeEquivalentTo(1);
        }

    }
}

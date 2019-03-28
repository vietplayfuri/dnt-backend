namespace costs.net.core.tests.Events.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Builders;
    using core.Events.Cost;
    using core.Services.PurchaseOrder;
    using core.Services.Search;
    using dataAccess.Entity;
    using messaging.Handlers.Events;
    using Serilog;
    using core.Models;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CostStageRevisionStatusChangedHandlerTests
    {
        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger>();
            _paperpusherNotifierMock = new Mock<IPaperpusherNotifier>();
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _paperpusherNotifierBuilders = new List<Lazy<IPaperpusherNotifier, PluginMetadata>>();

            _handler = new CostStageRevisionStatusChangedHandler(
                _loggerMock.Object,
                _paperpusherNotifierBuilders,
                _elasticSearchServiceMock.Object
            );

            _paperpusherNotifierBuilders.Add(new Lazy<IPaperpusherNotifier, PluginMetadata>(
                () => _paperpusherNotifierMock.Object,
                new PluginMetadata { BuType = BuType.Pg })
            );

            _costStageRevisionId = Guid.NewGuid();
        }

        private Mock<ILogger> _loggerMock;
        private Mock<IPaperpusherNotifier> _paperpusherNotifierMock;
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private List<Lazy<IPaperpusherNotifier, PluginMetadata>> _paperpusherNotifierBuilders;
        private Guid _costStageRevisionId;

        private CostStageRevisionStatusChangedHandler _handler;

        private CostStageRevisionStatusChanged GetStatusChangedEvent(CostStageRevisionStatus status)
        {
            return new CostStageRevisionStatusChanged
            {
                AggregateId = Guid.NewGuid(),
                CostStageRevisionId = _costStageRevisionId,
                Status = status,
                BuType = BuType.Pg,
                TimeStamp = DateTime.UtcNow
            };
        }

        [Test]
        public async Task Handle_always_updateIndexInElasticSearch()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(It.IsAny<CostStageRevisionStatus>());

            // Act
            await _handler.Handle(statusChanged);

            // Assert
            _elasticSearchServiceMock.Verify(p => p.UpdateCostSearchItem(It.Is<CostStageRevisionStatusChanged>(s => s == statusChanged)), Times.Once);
        }

        [Test]
        public async Task Handle_always_invokePaperpusherNotifier()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(It.IsAny<CostStageRevisionStatus>());

            // Act
            await _handler.Handle(statusChanged);

            // Assert
            _paperpusherNotifierMock.Verify(p => p.Notify(It.Is<CostStageRevisionStatusChanged>(s => s == statusChanged)), Times.Once);
        }
    }
}
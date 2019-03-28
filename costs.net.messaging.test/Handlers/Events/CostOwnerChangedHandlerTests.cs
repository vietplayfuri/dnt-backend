namespace costs.net.messaging.test.Handlers.Events
{
    using System;
    using System.Threading.Tasks;
    using core.Events.Cost;
    using core.Models.User;
    using core.Services.Search;
    using FluentAssertions;
    using messaging.Handlers.Events;
    using Moq;
    using NUnit.Framework;

    public class CostOwnerChangedHandlerTests
    {
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private CostOwnerChangedHandler _costOwnerChangedHandler;

        [SetUp]
        public void Init()
        {
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _costOwnerChangedHandler = new CostOwnerChangedHandler(_elasticSearchServiceMock.Object);
        }

        [Test]
        public async Task Handle_WhenEventIsValid_Should_UpdateCostInElasticSearch()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costOwnerChanged = new CostOwnerChanged { AggregateId = costId, UserIdentity = new UserIdentity() };
             
            // Act
            await _costOwnerChangedHandler.Handle(costOwnerChanged);

            // Assert
            _elasticSearchServiceMock.Verify(a => a.UpdateCostSearchItem(It.Is<CostOwnerChanged>(e => e.AggregateId == costId)));
        }

        [Test]
        public void Handle_WhenEventIsNull_ShouldThrowException()
        {
            // Act
            // Assert
            _costOwnerChangedHandler.Awaiting(h => h.Handle(null)).ShouldThrow<ArgumentNullException>();
        }
    }
}
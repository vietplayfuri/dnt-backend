namespace costs.net.core.tests.Events.Handlers
{
    using System;
    using System.Threading.Tasks;
    using core.Events.Cost;
    using core.Services.Search;
    using messaging.Handlers.Events;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ApproversUpdatedHandlerTests
    {
        [SetUp]
        public void Init()
        {
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();

            _handler = new ApproversUpdatedHandler(_elasticSearchServiceMock.Object);
        }

        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private ApproversUpdatedHandler _handler;

        [Test]
        public async Task Handle_always_shouldUpdateIndexInElasticSearch()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var approversUpdate = new ApproversUpdated(costId);

            // Act
            await _handler.Handle(approversUpdate);

            // Assert
            _elasticSearchServiceMock.Verify(es => es.UpdateCostSearchItem(It.Is<ApproversUpdated>(e => e.AggregateId == costId)), Times.Once);
        }
    }
}
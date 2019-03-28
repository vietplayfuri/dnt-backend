namespace costs.net.dataAccess.tests
{
    using System;
    using System.Threading.Tasks;
    using Entity;
    using FluentAssertions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class EFContextTests
    {
        [SetUp]
        public void Init()
        {
            _systemUserId = Guid.NewGuid();
            _efContext = EFContextFactory.CreateInMemoryEFContext(_systemUserId);
        }

        private Guid _systemUserId;
        private EFContext _efContext;

        [Test]
        public void SaveChanges_Always_Should_SetModifiedDateOnModifiedEntities()
        {
            // Arrange
            var existingCostId = Guid.NewGuid();
            var cost = new Cost { Id = existingCostId };
            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            // Act
            cost.Deleted = true; // not cost marked as modified
            var timeStamp = DateTime.UtcNow;
            _efContext.SaveChanges();

            // Assert
            cost.Modified.Should().NotBeNull();
            cost.Modified.Should().BeAfter(timeStamp);
        }

        [Test]
        public void SaveChanges_WhenAddedAndCreateByIsEmpty_Should_SetCreatedByAndCreatedOnAddedEntities()
        {
            // Arrange
            var existingCostId = Guid.NewGuid();
            var cost = new Cost { Id = existingCostId };
            _efContext.Cost.Add(cost);

            // Act
            var timeStamp = DateTime.UtcNow;
            _efContext.SaveChanges();

            // Assert
            cost.CreatedById.Should().Be(_systemUserId);
            cost.Created.Should().BeAfter(timeStamp);
        }

        [Test]
        public void SaveChanges_WhenAddedAndCreatedByIsNotEmpty_ShouldNot_SetCreatedByAndCreatedOnAddedEntities()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingCostId = Guid.NewGuid();
            var cost = new Cost(userId) { Id = existingCostId };
            _efContext.Cost.Add(cost);

            // Act
            var timeStamp = DateTime.UtcNow;
            _efContext.SaveChanges();

            // Assert
            cost.CreatedById.Should().Be(userId);
            cost.Created.Should().BeBefore(timeStamp);
        }

        [Test]
        public async Task SaveChangesAsync_Always_Should_SetModifiedDateOnModifiedEntities()
        {
            // Arrange
            var existingCostId = Guid.NewGuid();
            var cost = new Cost { Id = existingCostId };
            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            // Act
            cost.Deleted = true; // not cost marked as modified
            var timeStamp = DateTime.UtcNow;
            await _efContext.SaveChangesAsync();

            // Assert
            cost.Modified.Should().NotBeNull();
            cost.Modified.Should().BeAfter(timeStamp);
        }

        [Test]
        public async Task SaveChangesAsync_WhenAddedAndCreateByIsEmpty_Should_SetCreatedByAndCreatedOnAddedEntities()
        {
            // Arrange
            var existingCostId = Guid.NewGuid();
            var cost = new Cost { Id = existingCostId };
            _efContext.Cost.Add(cost);

            // Act
            var timeStamp = DateTime.UtcNow;
            await _efContext.SaveChangesAsync();

            // Assert
            cost.CreatedById.Should().Be(_systemUserId);
            cost.Created.Should().BeAfter(timeStamp);
        }

        [Test]
        public async Task SaveChangesAsync_WhenAddedAndCreatedByIsNotEmpty_ShouldNot_SetCreatedByAndCreatedOnAddedEntities()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingCostId = Guid.NewGuid();
            var cost = new Cost(userId) { Id = existingCostId };
            _efContext.Cost.Add(cost);

            // Act
            var timeStamp = DateTime.UtcNow;
            await _efContext.SaveChangesAsync();

            // Assert
            cost.CreatedById.Should().Be(userId);
            cost.Created.Should().BeBefore(timeStamp);
        }
    }
}
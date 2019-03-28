namespace costs.net.core.tests.Models
{
    using core.Models;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class BasePagingQueryTests
    {
        private BasePagingQuery _query;

        [Test]
        public void Skip_WhenPageNumberIsZero_ShouldBeZero()
        {
            // Arrange
            _query = new BasePagingQuery
            {
                PageNumber = 0,
                PageSize = 10
            };

            // Act
            var skip = _query.Skip;

            // Assert
            skip.Should().Be(0);
        }

        [Test]
        public void Skip_WhenPageNumberIsOne_ShouldBeZero()
        {
            // Arrange
            _query = new BasePagingQuery
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var skip = _query.Skip;

            // Assert
            skip.Should().Be(0);
        }

        [Test]
        public void Skip_WhenPageNumberIsMoreThenOne_ShouldBeCorrectlyCalculated()
        {
            // Arrange
            _query = new BasePagingQuery
            {
                PageNumber = 3,
                PageSize = 10
            };
            // 10 * (3 - 1) = 20
            const int expected = 20;

            // Act
            var skip = _query.Skip;

            // Assert
            skip.Should().Be(expected);
        }
    }
}

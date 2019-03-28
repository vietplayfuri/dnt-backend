namespace costs.net.core.tests.Helpers
{
    using core.Helpers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ExcelColumnNameTests
    {
        [Test]
        public void FirstColumn_ReturnsA()
        {
            const int columnIndex = 0;
            const string expected = "A";

            var result = ExcelColumnName.ColumnNameFromIndex(columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void SecondColumn_ReturnsB()
        {
            const int columnIndex = 1;
            const string expected = "B";

            var result = ExcelColumnName.ColumnNameFromIndex(columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void TwentySeventhColumn_ReturnsAA()
        {
            const int columnIndex = 26;
            const string expected = "AA";

            var result = ExcelColumnName.ColumnNameFromIndex(columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void TwentyEighthColumn_ReturnsAB()
        {
            const int columnIndex = 27;
            const string expected = "AB";

            var result = ExcelColumnName.ColumnNameFromIndex(columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void NegativeColumn_ReturnsEmpty()
        {
            const int columnIndex = -1;
            const string expected = "";

            var result = ExcelColumnName.ColumnNameFromIndex(columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }
    }
}

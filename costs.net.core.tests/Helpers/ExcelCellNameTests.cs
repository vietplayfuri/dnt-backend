namespace costs.net.core.tests.Helpers
{
    using core.Helpers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ExcelCellNameTests
    {
        [Test]
        public void FirstColumn_ReturnsA2()
        {
            const int rowIndex = 1;
            const int columnIndex = 0;
            const string expected = "A2";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void SecondColumn_ReturnsB2()
        {
            const int rowIndex = 1;
            const int columnIndex = 1;
            const string expected = "B2";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void TwentySeventhColumn_ReturnsAA2()
        {
            const int rowIndex = 1;
            const int columnIndex = 26;
            const string expected = "AA2";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void TwentyEighthColumn_ReturnsAB13()
        {
            const int rowIndex = 12;
            const int columnIndex = 27;
            const string expected = "AB13";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void NegativeRow_ReturnsEmpty()
        {
            const int rowIndex = -1;
            const int columnIndex = 1;
            const string expected = "";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void NegativeColumn_ReturnsEmpty()
        {
            const int rowIndex = 1;
            const int columnIndex = -1;
            const string expected = "";

            var result = ExcelCellName.CellNameFromIndex(rowIndex, columnIndex);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }
    }
}

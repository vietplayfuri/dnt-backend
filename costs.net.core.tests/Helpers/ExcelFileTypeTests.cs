
namespace costs.net.core.tests.Helpers
{
    using core.Helpers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ExcelFileTypeTests
    {
        [Test]
        public void IsExcelFileExtension_Null_ReturnsFalse()
        {
            string filename = null;
            const bool expected = false;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_EmptyString_ReturnsFalse()
        {
            string filename = string.Empty;
            const bool expected = false;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_NonExcelExtension_ReturnsFalse()
        {
            string filename = "file.txt";
            const bool expected = false;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_xlsxExtensionUppercase_ReturnsTrue()
        {
            string filename = "file.XLSX";
            const bool expected = true;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_xlsxExtensionLowercase_ReturnsTrue()
        {
            string filename = "file.xlsx";
            const bool expected = true;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_xlsExtensionUppercase_ReturnsTrue()
        {
            string filename = "file.XLS";
            const bool expected = true;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsExcelFileExtension_xlsExtensionLowercase_ReturnsTrue()
        {
            string filename = "file.xls";
            const bool expected = true;

            bool result = ExcelFileType.IsExcelFileExtension(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_Null_ReturnsFalse()
        {
            string filename = null;
            const bool expected = false;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_EmptyString_ReturnsFalse()
        {
            string filename = string.Empty;
            const bool expected = false;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_NonExcelExtension_ReturnsFalse()
        {
            string filename = "file.txt";
            const bool expected = false;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_xlsxExtensionUppercase_ReturnsTrue()
        {
            string filename = "file.XLSX";
            const bool expected = true;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_xlsxExtensionLowercase_ReturnsTrue()
        {
            string filename = "file.xlsx";
            const bool expected = true;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_xlsExtensionUppercase_ReturnsFalse()
        {
            string filename = "file.XLS";
            const bool expected = false;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsOpenXml_xlsExtensionLowercase_ReturnsFalse()
        {
            string filename = "file.xls";
            const bool expected = false;

            bool result = ExcelFileType.IsOpenXml(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_Null_ReturnsFalse()
        {
            string filename = null;
            const bool expected = false;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_EmptyString_ReturnsFalse()
        {
            string filename = string.Empty;
            const bool expected = false;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_NonExcelExtension_ReturnsFalse()
        {
            string filename = "file.txt";
            const bool expected = false;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_xlsxExtensionUppercase_ReturnsFalse()
        {
            string filename = "file.XLSX";
            const bool expected = false;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_xlsxExtensionLowercase_ReturnsFalse()
        {
            string filename = "file.xlsx";
            const bool expected = false;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_xlsExtensionUppercase_ReturnsTrue()
        {
            string filename = "file.XLS";
            const bool expected = true;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }

        [Test]
        public void IsBinary_xlsExtensionLowercase_ReturnsTrue()
        {
            string filename = "file.xls";
            const bool expected = true;

            bool result = ExcelFileType.IsBinary(filename);

            result.Should().Be(expected);
        }
    }
}

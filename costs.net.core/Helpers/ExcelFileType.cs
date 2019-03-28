
namespace dnt.core.Helpers
{
    /// <summary>
    /// Used to check the Excel file type based on the file extension.
    /// </summary>
    public class ExcelFileType
    {
        public static bool IsExcelFileExtension(string filename)
        {
            return IsOpenXml(filename) || IsBinary(filename);
        }

        public static bool IsOpenXml(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            return filename.ToLowerInvariant().EndsWith(".xlsx");
        }

        public static bool IsBinary(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            return filename.ToLowerInvariant().EndsWith(".xls");
        }
    }
}

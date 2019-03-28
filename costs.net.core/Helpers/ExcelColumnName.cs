using System;

namespace dnt.core.Helpers
{
    public class ExcelColumnName
    {
        /// <summary>
        /// Converts a column number to column name (i.e. A, B, C..., AA, AB...)
        /// </summary>
        /// <param name="columnIndex">Zero-indexed index of the column. 0 index means column A in Excel</param>
        /// <returns>Column name</returns>
        public static string ColumnNameFromIndex(int columnIndex)
        {
            string columnName = string.Empty;

            columnIndex++; //Increment by one for the algorithm to work. 

            while (columnIndex > 0)
            {
                int remainder = (columnIndex - 1) % 26;
                columnName = Convert.ToChar('A' + remainder) + columnName;
                columnIndex = ((columnIndex - remainder) / 26);
            }

            return columnName;
        }
    }
}

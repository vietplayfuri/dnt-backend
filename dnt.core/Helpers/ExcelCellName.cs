namespace dnt.core.Helpers
{
    public static class ExcelCellName
    {       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex">0 index means first row in Excel</param>
        /// <param name="columnIndex">0 index means column A in Excel</param>
        /// <returns></returns>
        public static string CellNameFromIndex(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0)
            {
                return string.Empty;
            }

            if (columnIndex < 0)
            {
                return string.Empty;
            }

            return $"{ExcelColumnName.ColumnNameFromIndex(columnIndex)}{rowIndex+1}";
        }
    }
}

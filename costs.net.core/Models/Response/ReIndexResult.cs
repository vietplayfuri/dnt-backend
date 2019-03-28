namespace dnt.core.Models.Response
{
    public class ReIndexResult
    {
        public bool Valid { get; set; }

        public long Count { get; set; }

        public string IndexName { get; set; }

        public string Error { get; set; }

        public static ReIndexResult InvalidResult(string errorText, string indexName = null)
        {
            return new ReIndexResult
            {
                Valid = false,
                Error = errorText,
                IndexName = indexName
            };
        }

        public static ReIndexResult ValidResult(int count, string indexName = null)
        {
            return new ReIndexResult
            {
                Valid = true,
                Count = count,
                IndexName = indexName
            };
        }
    }
}

namespace dnt.core.Models
{
    public class BasePagingQuery : IPagingQuery
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public int Skip => ((PageNumber == 0 ? 1 : PageNumber) - 1) * PageSize;
    }
}
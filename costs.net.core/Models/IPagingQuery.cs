namespace dnt.core.Models
{
    public interface IPagingQuery
    {
        int PageSize { get; set; }

        int PageNumber { get; set; }
    }
}
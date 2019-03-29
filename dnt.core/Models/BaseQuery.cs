namespace dnt.core.Models
{
    using System.Collections.Generic;

    public class BaseQuery : BasePagingQuery
    {
        public string SearchText { get; set; }

        public bool AutoComplete { get; set; }

        public bool IncludeDeleted { get; set; }

        public string Id { get; set; }

        public List<string> Projection { get; set; }

        public List<string> SearchTerms { get; set; }

        public List<string> Ids { get; set; }

        public int Limit { get; set; }
    }
}

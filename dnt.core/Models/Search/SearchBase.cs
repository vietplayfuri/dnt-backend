
namespace dnt.core.Models.Search
{
    using System.Collections.Generic;

    public abstract class SearchBase
    {
        public string SearchText { get; set; }
        
        public List<string> Projection { get; set; }
        
        public int Limit { get; set; }
    }
}

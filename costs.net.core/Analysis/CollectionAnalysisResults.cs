
namespace dnt.core.Analysis
{
    using System.Collections.Generic;

    public class CollectionAnalysisResults<T> where T : class
    {
        public CollectionAnalysisResults()
        {
            Added = new List<T>();
            Removed = new List<T>();
            Unchanged = new List<T>();
        }
        public List<T> Added { get; }
        public List<T> Removed { get; }
        public List<T> Unchanged { get; }
    }
}

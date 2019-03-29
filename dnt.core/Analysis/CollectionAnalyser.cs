
namespace dnt.core.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class is used to compare two collections to see what's been added, removed or stayed the same.
    /// </summary>
    public class CollectionAnalyser
    {
        public CollectionAnalysisResults<T> Analyse<T>(IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> comparer) where T : class
        {
            var results = new CollectionAnalysisResults<T>();

            results.Added.AddRange(y.Except(x, comparer));
            results.Removed.AddRange(x.Except(y, comparer));
            results.Unchanged.AddRange(x.Intersect(y, comparer));

            return results;
        }
    }
}

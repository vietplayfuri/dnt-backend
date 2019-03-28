namespace dnt.core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class EnumerableExtensions
    {
        public static T Single<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, Func<string> errorBuilder)
        {
            var result = enumerable.SingleOrDefault(predicate);
            if (result == null)
            {
                throw new InvalidOperationException($"Expected exactly one {typeof(T).Name} but found 0. {errorBuilder()}");
            }

            return result;
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> enumerable)
        {
            return Task.WhenAll(enumerable);
        }

    }
}
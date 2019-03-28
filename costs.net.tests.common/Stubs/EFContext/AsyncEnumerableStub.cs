namespace costs.net.tests.common.Stubs.EFContext
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class AsyncEnumerableStub<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public AsyncEnumerableStub(IEnumerable<T> enumerable)
            : base(enumerable)
        {}

        public AsyncEnumerableStub(Expression expression)
            : base(expression)
        {}

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumeratorStub<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new AsyncQueryProviderStub<T>(this);
    }
}
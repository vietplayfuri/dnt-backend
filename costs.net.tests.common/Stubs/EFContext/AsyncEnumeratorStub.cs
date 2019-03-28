namespace costs.net.tests.common.Stubs.EFContext
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncEnumeratorStub<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public AsyncEnumeratorStub(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public T Current => _inner.Current;

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<int> Range(
            int start,
            int count)
        {
            var num = (long)start + count - 1L;

            if (count < 0 || num > 2147483647L)
            {
                throw Error.ArgumentOutOfRange("count");
            }

            return new RangeAsyncObservable(start, count);
        }

        private sealed class RangeAsyncObservable : AsyncObservableBase<int>
        {
            private readonly int start;

            private readonly int count;

            public RangeAsyncObservable(
                int start, 
                int count)
            {
                this.start = start;
                this.count = count;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<int> observer, 
                CancellationToken cancellationToken)
            {
                for (var i = 0; i < this.count; i++)
                {
                    await observer.OnNextAsync(start + i, cancellationToken);
                }

                return null;
            }
        }
    }
}

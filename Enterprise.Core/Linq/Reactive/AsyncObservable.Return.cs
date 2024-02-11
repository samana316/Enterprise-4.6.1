using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Return<TResult>(
            TResult value)
        {
            return new ReturnAsyncObservable<TResult>(value);
        }

        private class ReturnAsyncObservable<TResult> : AsyncObservableBase<TResult>
        {
            private readonly TResult value;

            public ReturnAsyncObservable(
                TResult value)
            {
                this.value = value;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                await observer.OnNextAsync(this.value, cancellationToken);

                return null;
            }
        }
    }
}

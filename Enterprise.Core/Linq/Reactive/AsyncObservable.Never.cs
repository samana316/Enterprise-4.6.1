using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Never<TResult>()
        {
            return new NeverAsyncObservable<TResult>();
        }

        private sealed class NeverAsyncObservable<TResult> : AsyncObservableBase<TResult>
        {
            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                while (true)
                {
                    await Task.Yield();
                }
            }
        }
    }
}

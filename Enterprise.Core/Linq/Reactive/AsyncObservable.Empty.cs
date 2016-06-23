using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Empty<TResult>()
        {
            return new EmptyAsyncObservable<TResult>();
        }

        private sealed class EmptyAsyncObservable<TResult> : AsyncObservableBase<TResult>
        {
            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                return TaskHelpers.Constant<IDisposable>(null);
            }
        }
    }
}

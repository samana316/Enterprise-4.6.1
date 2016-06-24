using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq.Reactive.Impl;

namespace Enterprise.Core.Linq.Reactive
{
    internal abstract class AsyncObservableBase<T> : AsyncObservableImplBase4<T>
    {
        internal Task<IDisposable> SubscribeRawAsync(
            IAsyncObserver<T> observer, 
            CancellationToken cancellationToken)
        {
            return this.SubscribeCoreAsync(observer, cancellationToken);
        }
    }
}

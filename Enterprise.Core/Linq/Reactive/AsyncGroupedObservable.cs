using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    internal sealed class AsyncGroupedObservable<TKey, TElement> : IAsyncGroupedObservable<TKey, TElement>
    {
        private readonly TKey key;

        private readonly IAsyncObservable<TElement> subject;

        public AsyncGroupedObservable(
            TKey key, 
            IAsyncObservable<TElement> subject)
        {
            this.key = key;
            this.subject = subject;
        }

        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        public IDisposable Subscribe(
            IObserver<TElement> observer)
        {
            return this.subject.Subscribe(observer);
        }

        public Task<IDisposable> SubscribeAsync(
            IAsyncObserver<TElement> observer, 
            CancellationToken cancellationToken)
        {
            return this.subject.SubscribeRawAsync(observer, cancellationToken);
        }
    }
}

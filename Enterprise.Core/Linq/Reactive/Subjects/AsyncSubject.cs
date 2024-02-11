using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;

namespace Enterprise.Core.Linq.Reactive.Subjects
{
    internal sealed class AsyncSubject<T> : DisposableBase, IAsyncSubject<T>
    {
        private volatile IAsyncObserver<T> observer = AsyncObserver.NoOp<T>();

        public void OnCompleted()
        {
            this.observer.OnCompleted();
        }

        public void OnError(
            Exception error)
        {
            this.observer.OnError(error);
        }

        public void OnNext(
            T value)
        {
            this.observer.OnNext(value);
        }

        public Task OnNextAsync(
            T value, 
            CancellationToken cancellationToken)
        {
            return this.observer.OnNextAsync(value, cancellationToken);
        }

        public IDisposable Subscribe(
            IObserver<T> observer)
        {
            return null;
        }

        public async Task<IDisposable> SubscribeAsync(
            IAsyncObserver<T> observer, 
            CancellationToken cancellationToken)
        {
            if (observer != null)
            {
                this.observer = observer;
            }

            await Task.Yield();

            return null;
        }
    }
}

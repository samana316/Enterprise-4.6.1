using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive.Impl
{
    internal sealed class WrapperObserver<T> : AsyncObserverBase<T>
    {
        private readonly IAsyncObserver<T> observer;

        internal bool disposed = false;

        internal Action cancel = () => { };

        public WrapperObserver(
            IAsyncObserver<T> observer)
        {
            this.observer = observer;
        }

        protected override void Dispose(
            bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            var disposable = this.observer as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }

            base.Dispose(disposing);
            this.disposed = true;

            if (this.cancel != null)
            {
                this.cancel();
            }
        }

        protected override void OnCompletedCore()
        {
            this.observer.OnCompleted();
            this.Dispose();
        }

        protected override void OnErrorCore(
            Exception error)
        {
            this.observer.OnError(error);
            this.OnCompletedCore();
        }

        protected override async Task OnNextCoreAsync(
            T value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await this.observer.OnNextAsync(value, cancellationToken);
            }
            catch (Exception exception)
            {
                this.OnError(exception);
            }

            await Task.Yield();
        }
    }
}

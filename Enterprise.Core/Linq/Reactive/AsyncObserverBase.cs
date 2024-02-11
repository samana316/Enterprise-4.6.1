using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;

namespace Enterprise.Core.Linq.Reactive
{
    internal abstract class AsyncObserverBase<T> : DisposableBase, IAsyncObserver<T>
    {
        private int isStopped;

        protected AsyncObserverBase()
        {
            this.isStopped = 0;
        }

        public void OnCompleted()
        {
            if (Interlocked.Exchange(ref this.isStopped, 1) == 0)
            {
                this.OnCompletedCore();
            }
        }

        public void OnError(
            Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (Interlocked.Exchange(ref this.isStopped, 1) == 0)
            {
                this.OnErrorCore(error);
            }
        }

        public void OnNext(
            T value)
        {
            this.OnNextAsync(value).Wait();
        }

        public async Task OnNextAsync(
            T value, 
            CancellationToken cancellationToken)
        {
            if (this.isStopped == 0)
            {
                await this.OnNextCoreAsync(value, cancellationToken);
            }
        }

        protected abstract void OnCompletedCore();

        protected abstract void OnErrorCore(Exception error);

        protected abstract Task OnNextCoreAsync(T value, CancellationToken cancellationToken);

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                this.isStopped = 1;
            }

            base.Dispose(disposing);
        }

        internal bool Fail(
            Exception error)
        {
            if (Interlocked.Exchange(ref this.isStopped, 1) == 0)
            {
                this.OnErrorCore(error);
                return true;
            }

            return false;
        }
    }
}

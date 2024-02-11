using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    internal abstract class AsyncSink : AsyncObserverBase<object>, IPartialObserver
    {
        private readonly IPartialObserver observer;

        public AsyncSink(
            IPartialObserver observer)
        {
            this.observer = observer;
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
            this.OnCompleted();
        }

        protected override Task OnNextCoreAsync(
            object value, 
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(
            bool disposing)
        {
            var disposable = this.observer as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal abstract class AsyncSink<TResult> : AsyncSink, IAsyncObserver<TResult>
    {
        protected AsyncSink(
            IPartialObserver observer)
            : base(observer)
        {
        }

        public void OnNext(
            TResult value)
        {
            throw new NotSupportedException();
        }

        public Task OnNextAsync(
            TResult value, 
            CancellationToken cancellationToken)
        {
            return base.OnNextAsync(value, cancellationToken);
        }

        protected override sealed Task OnNextCoreAsync(
            object value, 
            CancellationToken cancellationToken)
        {
            return this.OnNextCoreAsync((TResult)value, cancellationToken);
        }

        protected abstract Task OnNextCoreAsync(TResult value, CancellationToken cancellationToken);
    }
}

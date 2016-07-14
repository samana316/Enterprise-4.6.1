using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservableBase<T> : DisposableBase, IAsyncObservable<T>
    {
        [Obsolete]
        public IDisposable Subscribe(
            IObserver<T> observer)
        {
            Check.NotNull(observer, "observer");

            return this.SubscribeAsync(observer.AsAsyncObserver()).Result;
        }

        public Task<IDisposable> SubscribeAsync(
            IAsyncObserver<T> observer,
            CancellationToken cancellationToken)
        {
            Check.NotNull(observer, "observer");
            cancellationToken.ThrowIfCancellationRequested();

            var consumer = new Consumer(this, observer);

            return consumer.RunAsync(cancellationToken);
        }

        protected abstract Task<IDisposable> SubscribeCoreAsync(
            IAsyncObserver<T> observer, CancellationToken cancellationToken);

        private async Task<IDisposable> SubscribeSafeAsync(
            IAsyncObserver<T> observer,
            CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await this.SubscribeCoreAsync(observer, cancellationToken);

                return subscription ?? EmptyDisposable.Instance;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (UnsubscribeException exception)
            {
                observer.OnError(exception);
            }
            catch (Exception exception)
            {
                observer.OnError(exception);
            }
            finally
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (UnsubscribeException exception)
                {
                    observer.OnError(exception);
                }
            }

            return EmptyDisposable.Instance;
        }

        private sealed class Consumer : AsyncObserverBase<T>
        {
            private readonly AsyncObservableBase<T> source;

            private readonly IAsyncObserver<T> observer;

            private bool disposed;

            public Consumer(
                AsyncObservableBase<T> source,
                IAsyncObserver<T> observer)
            {
                this.source = source;
                this.observer = observer;
            }

            public Task<IDisposable> RunAsync(
                CancellationToken cancellationToken)
            {
                return this.source.SubscribeSafeAsync(this, cancellationToken);
            }

            protected override void OnCompletedCore()
            {
                try
                {
                    this.observer.OnCompleted();
                    this.Dispose();
                }
                finally
                {
                    throw new UnsubscribeException();
                }
            }

            protected override void OnErrorCore(
                Exception error)
            {
                try
                {
                    this.observer.OnError(error);
                    this.OnCompleted();
                }
                catch (UnsubscribeException)
                {
                    throw;
                }
            }

            protected override Task OnNextCoreAsync(
                T value,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return this.observer.OnNextAsync(value, cancellationToken);
            }

            protected override void Dispose(
                bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;

                var disposable = this.observer as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private sealed class EmptyDisposable : DisposableBase
        {
            public static readonly IDisposable Instance = new EmptyDisposable();

            private EmptyDisposable()
            {
            }
        }

        private sealed class UnsubscribeException : Exception
        {
            public UnsubscribeException()
            {
            }

            public UnsubscribeException(string message)
                : base(message)
            {
            }

            public UnsubscribeException(
                string message,
                Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}

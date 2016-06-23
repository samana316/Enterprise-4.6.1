using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive.Impl
{
    internal abstract class AsyncObservableImplBase3<T> : DisposableBase, IAsyncObservable<T>
    {
        public IDisposable Subscribe(
            IObserver<T> observer)
        {
            throw new NotImplementedException();
        }

        public async Task<IDisposable> SubscribeAsync(
            IAsyncObserver<T> observer, 
            CancellationToken cancellationToken)
        {
            Check.NotNull(observer, "observer");
            cancellationToken.ThrowIfCancellationRequested();

            return await new AwaitableSubscription(this, observer, cancellationToken);
        }

        protected abstract Task<IDisposable> SubscribeCoreAsync(
            IAsyncObserver<T> observer, CancellationToken cancellationToken);

        private async Task<IDisposable> SubscribeSafeAsync(
            IAsyncObserver<T> observer, 
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.SubscribeCoreAsync(observer, cancellationToken);
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

            return null;
        }

        private sealed class AwaitableSubscription : AsyncObserverBase<T>
        {
            private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            private readonly IAsyncObserver<T> observer;

            internal readonly Task<IDisposable> subscribeTask;

            private bool disposed;

            public AwaitableSubscription(
                AsyncObservableImplBase3<T> source,
                IAsyncObserver<T> observer,
                CancellationToken cancellationToken)
            {
                this.observer = observer;
                this.subscribeTask = source.SubscribeSafeAsync(this, cancellationToken);
            }

            public TaskAwaiter<IDisposable> GetAwaiter()
            {
                return this.subscribeTask.GetAwaiter();
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

            protected override async Task OnNextCoreAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                this.cancellationTokenSource.Token.ThrowIfCancellationRequested();
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await this.observer.OnNextAsync(value, cancellationToken);
                }
                catch (UnsubscribeException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    this.OnError(exception);
                }
            }

            protected override void Dispose(
                bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;

                try
                {
                    this.cancellationTokenSource.Cancel();
                }
                catch (Exception exception)
                {
                    this.OnError(exception);
                }

                try
                {
                    if (this.subscribeTask != null)
                    {
                        this.subscribeTask.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    this.OnError(exception);
                }

                var disposable = this.observer as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                if (this.subscribeTask != null && 
                    this.subscribeTask.Status == TaskStatus.RanToCompletion)
                {
                    var result = this.subscribeTask.Result;

                    if (result != null)
                    {
                        result.Dispose();
                    }
                }

                this.cancellationTokenSource.Dispose();

                base.Dispose(disposing);
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

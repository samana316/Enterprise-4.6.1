using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Common.Threading;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive.Impl
{
    internal abstract class AsyncObservableImplBase1<T> : DisposableBase, IAsyncObservable<T>
    {
        internal bool disposed = false;

        private Subscription subscription;

        private WrapperObserver<T> observer;

        private Task<IDisposable> subscribeTask;

        private CancellationToken cancellationToken;

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
            this.cancellationToken = cancellationToken;

            this.observer = new WrapperObserver<T>(observer);
            this.subscription = new Subscription();

            this.subscription.cancel += () =>
            {
                if (this.observer != null && !this.observer.disposed)
                {
                    this.observer.Dispose();
                }
            };

            this.subscribeTask = this.SubscribeSafeAsync(this.observer, cancellationToken);

            this.observer.cancel += () =>
            {
                if (this.subscribeTask != null && (new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion }).Contains(this.subscribeTask.Status))
                {
                    this.subscribeTask.Dispose();
                }
                else
                {
                    var cancellationTokenSource = this.cancellationToken.GetSource();

                    try
                    {
                        if (cancellationTokenSource == null)
                        {
                            this.cancellationToken.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            cancellationTokenSource.Cancel();
                        }
                    }
                    catch (Exception exception)
                    {
                        this.observer.OnError(exception);
                    }

                    Task.Delay(10).Wait();

                    try
                    {
                        this.subscribeTask.Dispose();
                    }
                    catch (Exception exception)
                    {
                        this.observer.OnError(exception);
                    }

                    this.subscribeTask = null;
                }
            };

            var innerSubscription = await this.subscribeTask;

            this.subscription.cancel += () =>
            {
                if (innerSubscription != null)
                {
                    innerSubscription.Dispose();
                }
            };

            return this.subscription;
        }

        protected override void Dispose(
            bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (this.subscription != null && !this.subscription.disposed)
            {
                this.subscription.Dispose();
            }

            base.Dispose(disposing);
            this.disposed = true;
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
            catch (Exception exception)
            {
                observer.OnError(exception);

                return new Subscription();
            }
            finally
            {
                observer.OnCompleted();
            }
        }

        protected sealed class Subscription : DisposableBase
        {
            internal bool disposed = false;

            internal Action cancel = () => { };

            protected override void Dispose(
                bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                if (this.cancel != null)
                {
                    this.cancel();
                }

                base.Dispose(disposing);
                this.disposed = true;
            }
        }
    }
}

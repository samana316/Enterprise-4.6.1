using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Common.Threading;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive.Impl
{
    internal abstract class AsyncObservableImplBase2<T> : DisposableBase, IAsyncObservable<T>
    {
        private const double delay = 1;

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

            var subscription = new Subscription(this);
            var safeObserver = new WrapperObserver<T>(observer);

            try
            {
                while (await subscription.MoveNextAsync(cancellationToken))
                {
                    await safeObserver.OnNextAsync(subscription.Current, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                safeObserver.OnError(exception);
            }
            finally
            {
                safeObserver.OnCompleted();
            }

            return subscription;
        }

        protected internal IAsyncEnumerator<T> InternalGetAsyncEnumerator()
        {
            return new Subscription(this);
        }

        protected abstract Task<IDisposable> SubscribeCoreAsync(
            IAsyncObserver<T> observer, CancellationToken cancellationToken);

        private sealed class Subscription : DisposableBase, IAsyncEnumerator<T>
        {
            private readonly AsyncObservableImplBase2<T> source;

            private readonly PauseTokenSource pauseTokenSource = new PauseTokenSource();

            private readonly Consumer<T> consumer;

            private Task currentTask;

            public Subscription(
                AsyncObservableImplBase2<T> source)
            {
                this.source = source;
                this.consumer = new Consumer<T>(this.pauseTokenSource);
            }

            public T Current
            {
                get { return this.consumer.Current; }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                return this.MoveNextAsync().Result;
            }

            public async Task<bool> MoveNextAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (this.currentTask == null)
                {
                    this.pauseTokenSource.IsPaused = true;
                    this.currentTask = this.source.SubscribeCoreAsync(this.consumer, cancellationToken)
                        .ContinueWith(t => this.consumer.OnCompleted());
                }
                else
                {
                    this.ValidateCurrentTaskStatus();
                }

                if (this.pauseTokenSource.IsPaused)
                {
                    await this.pauseTokenSource.Token.WaitWhilePausedAsync(cancellationToken);
                    this.pauseTokenSource.IsPaused = true;
                }

                return this.consumer.HasValue;
            }

            public void Reset()
            {
                this.currentTask = null;
                this.consumer.Reset();
            }

            private void ValidateCurrentTaskStatus()
            {
                if (this.currentTask.Status == TaskStatus.RanToCompletion)
                {
                    this.pauseTokenSource.IsPaused = false;
                }

                if (this.currentTask.Status == TaskStatus.Canceled)
                {
                    throw new TaskCanceledException();
                }

                if (this.currentTask.Status == TaskStatus.Faulted)
                {
                    throw new AggregateException(this.currentTask.Exception.InnerExceptions);
                }
            }
        }

        private sealed class Consumer<TResult> : AsyncObserverBase<TResult>
        {
            public Consumer(
                PauseTokenSource pauseTokenSource)
            {
                this.CurrentIndex = -1;
                this.PauseTokenSource = pauseTokenSource;
            }

            public TResult Current { get; private set; }

            public bool HasValue { get; private set; }

            public int CurrentIndex { get; private set; }

            internal PauseTokenSource PauseTokenSource { get; set; }

            protected override async Task OnNextCoreAsync(
                TResult value,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
                if (this.HasValue)
                {
                    throw new InvalidOperationException(
                        "Yielded additional value before MoveNext(). This is probably caused by a missing await.");
                }

                this.Current = value;
                this.CurrentIndex++;

                var tcs = new TaskCompletionSource<TResult>();
                tcs.SetResult(value);

                this.HasValue = true;

                await tcs.Task
                    .ContinueWith(t => this.PauseTokenSource.IsPaused = false)
                    .ContinueWith(t => this.HasValue = false);
            }

            protected override void OnCompletedCore()
            {
                Task.Delay(TimeSpan.FromMilliseconds(delay)).Wait();
                this.HasValue = false;

                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);

                tcs.Task
                    .ContinueWith(t => this.PauseTokenSource.IsPaused = false)
                    .ContinueWith(t => this.HasValue = false).Wait();
            }

            protected override void OnErrorCore(
                Exception error)
            {
                TaskHelpers.ThrowAsync<TResult>(error).Wait();
            }

            public void Reset()
            {
                this.CurrentIndex = -1;
            }
        }
    }
}

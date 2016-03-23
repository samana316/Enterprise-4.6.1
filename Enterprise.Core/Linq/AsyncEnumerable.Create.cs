using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading;
using Enterprise.Core.Common.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Create<T>(
            Func<IAsyncYielder<T>, Task> yieldBuilder)
        {
            Func<IAsyncYielder<T>, CancellationToken, Task> overload = 
                (yielder, cancellationToken) => Task.Run(() => yieldBuilder(yielder), cancellationToken);

            return new AnonymousAsyncIterator<T>(overload);
        }

        public static IAsyncEnumerable<T> Create<T>(
            Func<IAsyncYielder<T>, CancellationToken, Task> yieldBuilder)
        {
            return new AnonymousAsyncIterator<T>(yieldBuilder);
        }

        internal static IAsyncEnumerable<T> CreateBufferred<T>(
            Func<IAsyncYielder<T>, CancellationToken, Task> yieldBuilder)
        {
            return new BufferedAnonymousAsyncIterator<T>(yieldBuilder);
        }

        private class AnonymousAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            private readonly Func<IAsyncYielder<TSource>, CancellationToken, Task> yieldBuilder;

            private readonly PauseTokenSource pauseTokenSource = new PauseTokenSource();

            private readonly Consumer<TSource> consumer;

            private Task currentTask;

            public AnonymousAsyncIterator(
                Func<IAsyncYielder<TSource>, CancellationToken, Task> yieldBuilder)
            {
                if (yieldBuilder == null)
                    throw new ArgumentNullException("yieldBuilder");

                this.yieldBuilder = yieldBuilder;
                this.consumer = new Consumer<TSource>(this.pauseTokenSource);
            }

            public override TSource Current
            {
                get { return this.consumer.Current; }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new AnonymousAsyncIterator<TSource>(this.yieldBuilder);
            }

            public override void Reset()
            {
                this.currentTask = null;
                this.consumer.Reset();

                base.Reset();
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (this.currentTask == null)
                {
                    this.pauseTokenSource.IsPaused = true;
                    this.currentTask = this.yieldBuilder(this.consumer, cancellationToken)
                        .ContinueWith(t => this.consumer.BreakAsync(cancellationToken));
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

            private sealed class Consumer<TResult> : IAsyncYielder<TResult>
            {
                private const double delay = 1;

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

                public async Task ReturnAsync(
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

                public async Task BreakAsync(
                    CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
                    this.HasValue = false;

                    var tcs = new TaskCompletionSource<object>();
                    tcs.SetResult(null);

                    await tcs.Task
                        .ContinueWith(t => this.PauseTokenSource.IsPaused = false)
                        .ContinueWith(t => this.HasValue = false);
                }

                public void Reset()
                {
                    this.CurrentIndex = -1;
                }
            }
        }

        private class BufferedAnonymousAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            private readonly Func<IAsyncYielder<TSource>, CancellationToken, Task> yieldBuilder;

            private readonly Consumer<TSource> consumer = new Consumer<TSource>();

            private int currentIndex = -1;

            private Task currentTask;

            public BufferedAnonymousAsyncIterator(
                Func<IAsyncYielder<TSource>, CancellationToken, Task> yieldBuilder)
            {
                if (yieldBuilder == null)
                    throw new ArgumentNullException("yieldBuilder");

                this.yieldBuilder = yieldBuilder;
            }

            public override TSource Current
            {
                get { return this.consumer.Items[this.currentIndex]; }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new BufferedAnonymousAsyncIterator<TSource>(this.yieldBuilder);
            }

            public override void Reset()
            {
                this.currentTask = null;
                this.currentIndex = -1;
                this.consumer.Reset();

                base.Reset();
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.currentTask == null)
                {
                    this.currentTask = this.yieldBuilder(this.consumer, cancellationToken);
                }

                this.currentIndex++;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (this.currentTask.Status == TaskStatus.Canceled)
                    {
                        throw new TaskCanceledException();
                    }

                    if (this.currentTask.Status == TaskStatus.Faulted)
                    {
                        throw new AggregateException(this.currentTask.Exception.InnerExceptions);
                    }

                    await Task.Delay(1);

                    if (this.currentIndex < this.consumer.Items.Count)
                    {
                        return true;
                    }
                }
                while (this.currentIndex >= this.consumer.Items.Count 
                    && this.currentTask.Status != TaskStatus.RanToCompletion);

                return false;
            }

            private class Consumer<TResult> : IAsyncYielder<TResult>
            {
                private readonly List<TResult> items = new List<TResult>();

                public IReadOnlyList<TResult> Items
                {
                    get { return this.items; }
                }

                public Task BreakAsync(
                    CancellationToken cancellationToken)
                {
                    this.items.Capacity = this.items.Count;

                    return TaskHelpers.Constant(false);
                }

                public Task ReturnAsync(
                    TResult value, 
                    CancellationToken cancellationToken)
                {
                    this.items.Add(value);

                    return TaskHelpers.Constant(true);
                }

                public void Reset()
                {
                    this.items.Clear();
                }
            }
        }
    }
}

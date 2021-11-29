using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Runtime.ExceptionServices;
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

            private Consumer _yield;
            private TSource _current;
            private Task _enumerationTask;
            private Exception _enumerationException;

            public AnonymousAsyncIterator(
                Func<IAsyncYielder<TSource>, CancellationToken, Task> yieldBuilder)
            {
                this.yieldBuilder = yieldBuilder;

                ClearState();
            }

            public override TSource Current
            {
                get
                {
                    if (_enumerationTask == null)
                        throw new InvalidOperationException("Call MoveNext() or MoveNextAsync() before accessing the Current item");
                    return _current;
                }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new AnonymousAsyncIterator<TSource>(this.yieldBuilder);
            }

            protected override void Dispose(
                bool disposing)
            {
                ClearState();

                base.Dispose(disposing);
            }

            public override void Reset()
            {
                ClearState();

                base.Reset();
            }

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (_enumerationException != null)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    tcs.SetException(_enumerationException);
                    return tcs.Task;
                }
                var moveNextTask = _yield.OnMoveNext(cancellationToken).ContinueWith(OnMoveNextComplete, _yield);
                if (_enumerationTask == null)
                    _enumerationTask = this.yieldBuilder(_yield, cancellationToken).ContinueWith(OnEnumerationComplete, _yield);
                return moveNextTask;
            }

            private void ClearState()
            {
                if (_yield != null)
                    _yield.Finilize();

                _yield = new Consumer();
                _enumerationTask = null;
                _enumerationException = null;
            }

            private bool OnMoveNextComplete(
                Task<TSource> task, 
                object state)
            {
                var yield = (Consumer)state;
                if (yield.IsComplete)
                {
                    return false;
                }

                if (task.IsFaulted)
                {
                    _enumerationException = task.Exception;
                    _enumerationException.Rethrow();
                }
                else if (task.IsCanceled)
                {
                    return false;
                }

                _current = task.Result;
                return true;
            }

            private static void OnEnumerationComplete(
                Task task, 
                object state)
            {
                var yield = (Consumer)state;
                if (task.IsFaulted)
                {
                    if (task.Exception is AsyncEnumerationCanceledException)
                    {
                        yield.SetCanceled();
                    }
                    else {
                        yield.SetFailed(task.Exception);
                    }
                }
                else if (task.IsCanceled)
                {
                    yield.SetCanceled();
                }
                else {
                    yield.SetComplete();
                }
            }

            private sealed class AsyncEnumerationCanceledException : OperationCanceledException { }

            private sealed class Consumer : IAsyncYielder<TSource>
            {
                private TaskCompletionSource<bool> _resumeTCS;
                private TaskCompletionSource<TSource> _yieldTCS = new TaskCompletionSource<TSource>();

                public CancellationToken CancellationToken { get; private set; }

                public async Task BreakAsync(
                    CancellationToken cancellationToken)
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();

                    SetCanceled();
                    throw new AsyncEnumerationCanceledException();
                }

                public Task ReturnAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _resumeTCS = new TaskCompletionSource<bool>();
                    _yieldTCS.TrySetResult(value);
                    return _resumeTCS.Task;
                }

                internal void SetComplete()
                {
                    _yieldTCS.TrySetCanceled();
                    IsComplete = true;
                }

                internal void SetCanceled()
                {
                    SetComplete();
                }

                internal void SetFailed(Exception ex)
                {
                    _yieldTCS.TrySetException(ex);
                    IsComplete = true;
                }

                internal Task<TSource> OnMoveNext(
                    CancellationToken cancellationToken)
                {
                    if (!IsComplete)
                    {
                        _yieldTCS = new TaskCompletionSource<TSource>();
                        CancellationToken = cancellationToken;
                        if (_resumeTCS != null)
                            _resumeTCS.SetResult(true);
                    }
                    return _yieldTCS.Task;
                }

                internal void Finilize()
                {
                    SetCanceled();
                }

                internal bool IsComplete { get; set; }
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

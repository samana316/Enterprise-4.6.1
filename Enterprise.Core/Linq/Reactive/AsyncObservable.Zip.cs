using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Zip<TFirst, TSecond, TResult>(
            this IAsyncObservable<TFirst> first,
            IObservable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");
            Check.NotNull(resultSelector, "resultSelector");

            return new ZipAsyncObservable<TFirst, TSecond, TResult>(first, second, resultSelector);
        }

        public static IAsyncObservable<TResult> Zip<TFirst, TSecond, TResult>(
            this IAsyncObservable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");
            Check.NotNull(resultSelector, "resultSelector");

            return new ZipAsyncObservable<TFirst, TSecond, TResult>(first, second, resultSelector);
        }

        private sealed class ZipAsyncObservable<TFirst, TSecond, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TFirst> first;

            private readonly IAsyncObservable<TSecond> second;

            private readonly IAsyncEnumerable<TSecond> secondE;

            private readonly Func<TFirst, TSecond, TResult> resultSelector;

            public ZipAsyncObservable(
                IAsyncObservable<TFirst> first, 
                IObservable<TSecond> second, 
                Func<TFirst, TSecond, TResult> resultSelector)
            {
                this.first = first;
                this.second = second.AsAsyncObservable();
                this.resultSelector = resultSelector;
            }

            public ZipAsyncObservable(
                IAsyncObservable<TFirst> first, 
                IEnumerable<TSecond> second, 
                Func<TFirst, TSecond, TResult> resultSelector)
            {
                this.first = first;
                this.secondE = second.AsAsyncEnumerable();
                this.resultSelector = resultSelector;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                if (this.second != null)
                {
                    var zipImpl = new ZipAsyncObservableImpl(this, observer);

                    return zipImpl.RunAsync(cancellationToken);
                }

                var zipImplE = new ZipAsyncEnumerableObserver(this, observer);

                return this.first.SubscribeSafeAsync(zipImplE, cancellationToken);
            }

            private sealed class ZipAsyncEnumerableObserver : AsyncSink<TFirst>
            {
                private readonly ZipAsyncObservable<TFirst, TSecond, TResult> parent;

                private readonly IAsyncObserver<TResult> observer;

                private IAsyncEnumerator<TSecond> secondEnumerator;

                public ZipAsyncEnumerableObserver(
                    ZipAsyncObservable<TFirst, TSecond, TResult> parent, 
                    IAsyncObserver<TResult> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override void Dispose(
                    bool disposing)
                {
                    if (this.secondEnumerator != null)
                    {
                        this.secondEnumerator.Dispose();
                    }

                    base.Dispose(disposing);
                }

                protected override async Task OnNextCoreAsync(
                    TFirst value, 
                    CancellationToken cancellationToken)
                {
                    if (this.secondEnumerator == null)
                    {
                        this.secondEnumerator = this.parent.secondE.GetAsyncEnumerator();
                    }

                    if (await this.secondEnumerator.MoveNextAsync(cancellationToken))
                    {
                        var current = this.secondEnumerator.Current;
                        var result = this.parent.resultSelector(value, current);

                        await this.observer.OnNextAsync(result, cancellationToken);
                    }
                    else
                    {
                        this.OnCompleted();
                    }
                }
            }

            private sealed class ZipAsyncObservableImpl
            {
                private ZipAsyncObservable<TFirst, TSecond, TResult> parent;

                private IAsyncObserver<TResult> observer;

                public ZipAsyncObservableImpl(
                    ZipAsyncObservable<TFirst, TSecond, TResult> parent, 
                    IAsyncObserver<TResult> observer)
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                public Task<IDisposable> RunAsync(
                    CancellationToken cancellationToken)
                {
                    var enumerable = parent.second.ToAsyncEnumerable();

                    var observable = new ZipAsyncObservable<TFirst, TSecond, TResult>(
                        this.parent.first, enumerable, this.parent.resultSelector);

                    return observable.SubscribeSafeAsync(this.observer, cancellationToken);
                }
            }
        }
    }
}

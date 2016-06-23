using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Select<TSource, TResult>(
           this IAsyncObservable<TSource> source,
           Func<TSource, TResult> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, int, TResult> overload = (item, index) => selector(item);

            return new SelectAsyncObservable<TSource, TResult>(source, overload);
        }

        public static IAsyncObservable<TResult> Select<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, TResult> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SelectAsyncObservable<TSource, TResult>(source, selector);
        }

        private sealed class SelectAsyncObservable<TSource, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, int, TResult> selector;

            public SelectAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, int, TResult> selector)
            {
                this.source = source;
                this.selector = selector;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer,
                CancellationToken cancellationToken)
            {
                var selectImpl = new SelectAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(selectImpl, cancellationToken);
            }

            private sealed class SelectAsyncObserver : AsyncSink<TSource>
            {
                private readonly SelectAsyncObservable<TSource, TResult> parent;

                private readonly IAsyncObserver<TResult> observer;

                private int index;

                public SelectAsyncObserver(
                    SelectAsyncObservable<TSource, TResult> parent,
                    IAsyncObserver<TResult> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    var result = this.parent.selector(value, index++);

                    return this.observer.OnNextAsync(result, cancellationToken);
                }
            }
        }
    }
}

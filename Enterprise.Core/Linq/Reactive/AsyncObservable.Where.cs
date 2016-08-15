using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Where<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            Func<TSource, int, bool> overload = (item, index) => predicate(item);

            return new WhereAsyncObservable<TSource>(source, overload);
        }

        public static IAsyncObservable<TSource> Where<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new WhereAsyncObservable<TSource>(source, predicate);
        }

        private sealed class WhereAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, int, bool> predicate;

            public WhereAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, int, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var whereImpl = new WhereAsyncObserver(this, observer);

                return this.source.SubscribeRawAsync(whereImpl, cancellationToken);
            }

            private sealed class WhereAsyncObserver : AsyncSink<TSource>
            {
                private readonly WhereAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int index;

                public WhereAsyncObserver(
                    WhereAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    if (this.parent.predicate(value, this.index++))
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                }
            }
        }
    }
}

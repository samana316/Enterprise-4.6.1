using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<int> LongCount<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new LongCountAsyncObservable<TSource>(source, null);
        }

        public static IAsyncObservable<int> LongCount<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new LongCountAsyncObservable<TSource>(source, predicate);
        }

        private sealed class LongCountAsyncObservable<TSource> : AsyncObservableBase<int>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            public LongCountAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<int> observer,
                CancellationToken cancellationToken)
            {
                var count = 0;

                Action<TSource> onNext = null;

                if (this.predicate == null)
                {
                    onNext = value =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        count++;
                    };
                }
                else
                {
                    onNext = value =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (this.predicate(value))
                        {
                            count++;
                        }
                    };
                }

                await this.source.ForEachAsync(onNext, cancellationToken);
                await observer.OnNextAsync(count, cancellationToken);

                return null;
            }
        }
    }
}

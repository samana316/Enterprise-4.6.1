using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Concat<TSource>(
            this IAsyncObservable<TSource> first,
            IAsyncObservable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return Concat(new[] { first, second });
        }

        public static IAsyncObservable<TSource> Concat<TSource>(
            params IAsyncObservable<TSource>[] sources)
        {
            Check.NotNull(sources, "sources");

            return new ConcatAsyncObservable<TSource>(sources.AsAsyncEnumerable());
        }

        public static IAsyncObservable<TSource> Concat<TSource>(
            this IEnumerable<IAsyncObservable<TSource>> sources)
        {
            Check.NotNull(sources, "sources");

            return new ConcatAsyncObservable<TSource>(sources.AsAsyncEnumerable());
        }

        public static IAsyncObservable<TSource> Concat<TSource>(
            this IObservable<IAsyncObservable<TSource>> sources)
        {
            Check.NotNull(sources, "sources");

            return new ConcatAsyncObservable<TSource>(sources.AsAsyncObservable());
        }

        private sealed class ConcatAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<IAsyncObservable<TSource>> sources;

            private readonly IAsyncEnumerable<IAsyncObservable<TSource>> sourceCollection;

            public ConcatAsyncObservable(
                IAsyncEnumerable<IAsyncObservable<TSource>> sources)
            {
                this.sourceCollection = sources;
            }

            public ConcatAsyncObservable(
                IAsyncObservable<IAsyncObservable<TSource>> sources)
            {
                this.sources = sources;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                if (this.sources != null)
                {
                    await this.RunObservableAsync(observer, cancellationToken);
                }
                else if (this.sourceCollection != null)
                {
                    await this.RunEnumerableAsync(observer, cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                return null;
            }

            private async Task RunEnumerableAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                using (var enumerator = this.sourceCollection.GetAsyncEnumerator())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        var source = enumerator.Current;
                        Check.NotNull(source, "source");

                        await source.ForEachAsync(observer.OnNextAsync, cancellationToken);
                    }
                }
            }

            private async Task RunObservableAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                Func<IAsyncObservable<TSource>, CancellationToken, Task> onNextAsync = async (value, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    await value.SubscribeRawAsync(observer, ct);
                };

                await this.sources.ForEachAsync(onNextAsync, cancellationToken);
            }
        }
    }
}

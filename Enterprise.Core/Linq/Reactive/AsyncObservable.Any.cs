using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<bool> Any<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new AnyAsyncObservable<TSource>(source, null);
        }

        public static IAsyncObservable<bool> Any<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new AnyAsyncObservable<TSource>(source, predicate);
        }

        private sealed class AnyAsyncObservable<TSource> : AsyncObservableBase<bool>
        {
            private Func<TSource, bool> predicate;

            private IAsyncObservable<TSource> source;

            public AnyAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<bool> observer,
                CancellationToken cancellationToken)
            {
                if (this.predicate == null)
                {
                    return this.AnyAsync(observer, cancellationToken);
                }

                return this.AnyImplAsync(observer, cancellationToken);
            }

            private async Task<IDisposable> AnyAsync(
                IAsyncObserver<bool> observer,
                CancellationToken cancellationToken)
            {
                var result = false;
                var query = this.source.While(() => !result);

                await query.ForEachAsync(x => result = true, cancellationToken);
                await observer.OnNextAsync(result, cancellationToken);

                return null;
            }

            private async Task<IDisposable> AnyImplAsync(
                IAsyncObserver<bool> observer,
                CancellationToken cancellationToken)
            {
                var result = false;
                var query = this.source.While(() => !result);

                await query.ForEachAsync(x => result = predicate(x), cancellationToken);
                await observer.OnNextAsync(result, cancellationToken);

                return null;
            }
        }
    }
}

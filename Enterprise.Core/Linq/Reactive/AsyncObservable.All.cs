using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<bool> All<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new AllAsyncObservable<TSource>(source, predicate);
        }

        private sealed class AllAsyncObservable<TSource> : AsyncObservableBase<bool>
        {
            private Func<TSource, bool> predicate;

            private IAsyncObservable<TSource> source;

            public AllAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<bool> observer, 
                CancellationToken cancellationToken)
            {
                var result = true;
                var query = this.source.While(() => result);

                await query.ForEachAsync(x => result = predicate(x), cancellationToken);

                await observer.OnNextAsync(result, cancellationToken);

                return null;
            }
        }
    }
}

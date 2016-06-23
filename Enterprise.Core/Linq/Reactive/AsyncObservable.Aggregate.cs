using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TAccumulate> Aggregate<TSource, TAccumulate>(
            this IAsyncObservable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            Check.NotNull(source, "source");
            Check.NotNull(accumulator, "accumulator");

            return new AggregateAsyncObservable<TSource, TAccumulate, TAccumulate>(
                source, seed, accumulator, IdentityFunction);
        }

        public static IAsyncObservable<TResult> Aggregate<TSource, TAccumulate, TResult>(
            this IAsyncObservable<TSource> source, 
            TAccumulate seed, 
            Func<TAccumulate, TSource, TAccumulate> accumulator, 
            Func<TAccumulate, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(accumulator, "accumulator");
            Check.NotNull(resultSelector, "resultSelector");

            return new AggregateAsyncObservable<TSource, TAccumulate, TResult>(
                source, seed, accumulator, resultSelector);
        }

        public static IAsyncObservable<TSource> Aggregate<TSource>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TSource, TSource> accumulator)
        {
            Check.NotNull(source, "source");
            Check.NotNull(accumulator, "accumulator");

            return new AggregateAsyncObservable<TSource, TSource, TSource>(
                source, default(TSource), accumulator, IdentityFunction);
        }

        private sealed class AggregateAsyncObservable<TSource, TAccumulate, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly TAccumulate seed;

            private readonly Func<TAccumulate, TSource, TAccumulate> accumulator;

            private readonly Func<TAccumulate, TResult> resultSelector;

            public AggregateAsyncObservable(
                IAsyncObservable<TSource> source, 
                TAccumulate seed, 
                Func<TAccumulate, TSource, TAccumulate> accumulator, 
                Func<TAccumulate, TResult> resultSelector)
            {
                this.source = source;
                this.seed = seed;
                this.accumulator = accumulator;
                this.resultSelector = resultSelector;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                var accumulate = seed;

                await this.source.ForEachAsync(current =>
                {
                    accumulate = accumulator(accumulate, current);
                },
                cancellationToken);

                var result = Return(this.resultSelector(accumulate));

                return await result.SubscribeSafeAsync(observer, cancellationToken);
            }
        }
    }
}

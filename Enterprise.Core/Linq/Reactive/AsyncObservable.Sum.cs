using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<decimal> Sum(
            this IAsyncObservable<decimal> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservable<decimal>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<double> Sum(
            this IAsyncObservable<double> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservable<double>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<float> Sum(
            this IAsyncObservable<float> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservable<float>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<int> Sum(
            this IAsyncObservable<int> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservable<int>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<long> Sum(
            this IAsyncObservable<long> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservable<long>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<decimal?> Sum(
            this IAsyncObservable<decimal?> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservableNullable<decimal>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<double?> Sum(
            this IAsyncObservable<double?> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservableNullable<double>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<float?> Sum(
            this IAsyncObservable<float?> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservableNullable<float>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<int?> Sum(
            this IAsyncObservable<int?> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservableNullable<int>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<long?> Sum(
            this IAsyncObservable<long?> source)
        {
            Check.NotNull(source, "source");

            return new SumAsyncObservableNullable<long>(source, (x, y) => x + y);
        }

        public static IAsyncObservable<decimal> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, decimal> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservable<TSource, decimal>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<double> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservable<TSource, double>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<float> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, float> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservable<TSource, float>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<int> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservable<TSource, int>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<long> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, long> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservable<TSource, long>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<decimal?> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, decimal?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservableNullable<TSource, decimal>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<double?> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservableNullable<TSource, double>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<float?> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, float?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservableNullable<TSource, float>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<int?> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservableNullable<TSource, int>(source, (x, y) => x + y, selector);
        }

        public static IAsyncObservable<long?> Sum<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, long?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SumAsyncObservableNullable<TSource, long>(source, (x, y) => x + y, selector);
        }

        private sealed class SumAsyncObservable<TSource> : AsyncObservableBase<TSource>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, TSource, TSource> adder;

            public SumAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, TSource, TSource> adder)
            {
                this.source = source;
                this.adder = adder;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var sum = default(TSource);

                await source.ForEachAsync(value => sum = this.adder(value, sum), cancellationToken);
                await observer.OnNextAsync(sum, cancellationToken);

                return null;
            }
        }

        private sealed class SumAsyncObservable<TSource, TResult> : AsyncObservableBase<TResult>
            where TResult : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TResult, TResult, TResult> adder;

            private readonly Func<TSource, TResult> selector;

            public SumAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TResult, TResult, TResult> adder, 
                Func<TSource, TResult> selector)
            {
                this.source = source;
                this.adder = adder;
                this.selector = selector;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer,
                CancellationToken cancellationToken)
            {
                var sum = default(TResult);

                await source.ForEachAsync(value => sum = this.adder(selector(value), sum), cancellationToken);
                await observer.OnNextAsync(sum, cancellationToken);

                return null;
            }
        }

        private sealed class SumAsyncObservableNullable<TSource> : AsyncObservableBase<TSource?>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource?> source;

            private readonly Func<TSource?, TSource?, TSource?> adder;

            public SumAsyncObservableNullable(
                IAsyncObservable<TSource?> source,
                Func<TSource?, TSource?, TSource?> adder)
            {
                this.source = source;
                this.adder = adder;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource?> observer,
                CancellationToken cancellationToken)
            {
                TSource? sum = default(TSource);

                await source.ForEachAsync(
                    value => sum = this.adder(value.GetValueOrDefault(), sum.GetValueOrDefault()), cancellationToken);

                await observer.OnNextAsync(sum, cancellationToken);

                return null;
            }
        }

        private sealed class SumAsyncObservableNullable<TSource, TResult> : AsyncObservableBase<TResult?>
            where TResult : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TResult?, TResult?, TResult?> adder;

            private readonly Func<TSource, TResult?> selector;

            public SumAsyncObservableNullable(
                IAsyncObservable<TSource> source,
                Func<TResult?, TResult?, TResult?> adder,
                Func<TSource, TResult?> selector)
            {
                this.source = source;
                this.adder = adder;
                this.selector = selector;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult?> observer,
                CancellationToken cancellationToken)
            {
                TResult? sum = default(TResult);

                await source.ForEachAsync(
                    value => sum = this.adder(selector(value).GetValueOrDefault(), sum.GetValueOrDefault()), 
                    cancellationToken);

                await observer.OnNextAsync(sum, cancellationToken);

                return null;
            }
        }
    }
}

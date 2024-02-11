using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<decimal> Average(
            this IAsyncObservable<decimal> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservable<decimal>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<double> Average(
            this IAsyncObservable<double> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservable<double>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<float> Average(
            this IAsyncObservable<float> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservable<float>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<double> Average(
            this IAsyncObservable<int> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservable<int, double>(
                source, (x, y) => x + y, Convert.ToDouble, (x, y) => x / y);
        }

        public static IAsyncObservable<double> Average(
            this IAsyncObservable<long> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservable<long, double>(
                source, (x, y) => x + y, Convert.ToDouble, (x, y) => x / y);
        }

        public static IAsyncObservable<decimal?> Average(
            this IAsyncObservable<decimal?> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservableNullable<decimal>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average(
            this IAsyncObservable<double?> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservableNullable<double>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<float?> Average(
            this IAsyncObservable<float?> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservableNullable<float>(source, (x, y) => x + y, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average(
            this IAsyncObservable<int?> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservableNullable<int?, double>(source, (x, y) => x + y, ToDoubleNullable, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average(
            this IAsyncObservable<long?> source)
        {
            Check.NotNull(source, "source");

            return new AverageAsyncObservableNullable<long?, double>(source, (x, y) => x + y, ToDoubleNullable, (x, y) => x / y);
        }

        public static IAsyncObservable<decimal> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, decimal> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservable<TSource, decimal>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<double> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservable<TSource, double>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<float> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, float> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservable<TSource, float>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<double> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            throw new NotImplementedException();
        }

        public static IAsyncObservable<double> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, long> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            throw new NotImplementedException();
        }

        public static IAsyncObservable<decimal?> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, decimal?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservableNullable<TSource, decimal>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservableNullable<TSource, double>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<float?> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, float?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AverageAsyncObservableNullable<TSource, float>(source, (x, y) => x + y, selector, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, double?> overload = item =>
            {
                var value = selector(item);

                return ToDoubleNullable(value);
            };

            return new AverageAsyncObservableNullable<TSource, double>(source, (x, y) => x + y, overload, (x, y) => x / y);
        }

        public static IAsyncObservable<double?> Average<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, long?> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, double?> overload = item =>
            {
                var value = selector(item);

                return ToDoubleNullable(value);
            };

            return new AverageAsyncObservableNullable<TSource, double>(source, (x, y) => x + y, overload, (x, y) => x / y);
        }

        private sealed class AverageAsyncObservable<TSource> : AsyncObservableBase<TSource>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, TSource, TSource> adder;

            private readonly Func<TSource, long, TSource> divider;

            public AverageAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, TSource, TSource> adder,
                Func<TSource, long, TSource> divider)
            {
                this.source = source;
                this.adder = adder;
                this.divider = divider;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var sum = default(TSource);
                var count = 0L;

                Action<TSource> onNext = value =>
                {
                    sum = this.adder(value, sum);
                    count++;
                };

                await source.ForEachAsync(onNext, cancellationToken);

                if (count > 0)
                {
                    var average = this.divider(sum, count);

                    await observer.OnNextAsync(average, cancellationToken);

                    return null;
                }

                throw Error.EmptySequence();
            }
        }

        private sealed class AverageAsyncObservable<TSource, TResult> : AsyncObservableBase<TResult>
            where TResult : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TResult, TResult, TResult> adder;

            private readonly Func<TSource, TResult> selector;

            private readonly Func<TResult, long, TResult> divider;

            public AverageAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TResult, TResult, TResult> adder,
                Func<TSource, TResult> selector,
                Func<TResult, long, TResult> divider)
            {
                this.source = source;
                this.adder = adder;
                this.selector = selector;
                this.divider = divider;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer,
                CancellationToken cancellationToken)
            {
                var sum = default(TResult);
                var count = 0L;

                Action<TSource> onNext = value =>
                {
                    var result = this.selector(value);
                    sum = this.adder(result, sum);
                    count++;
                };

                await source.ForEachAsync(onNext, cancellationToken);

                if (count > 0)
                {
                    var average = this.divider(sum, count);

                    await observer.OnNextAsync(average, cancellationToken);

                    return null;
                }

                throw Error.EmptySequence();
            }
        }

        private sealed class AverageAsyncObservableNullable<TSource> : AsyncObservableBase<TSource?>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource?> source;

            private readonly Func<TSource?, TSource?, TSource?> adder;

            private readonly Func<TSource?, long, TSource?> divider;

            public AverageAsyncObservableNullable(
                IAsyncObservable<TSource?> source,
                Func<TSource?, TSource?, TSource?> adder,
                Func<TSource?, long, TSource?> divider)
            {
                this.source = source;
                this.adder = adder;
                this.divider = divider;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource?> observer,
                CancellationToken cancellationToken)
            {
                var sum = default(TSource?);
                var count = 0L;

                Action<TSource?> onNext = value =>
                {
                    if (value.HasValue)
                    {
                        sum = this.adder(value, sum.GetValueOrDefault());
                        count++;
                    }
                };

                await source.ForEachAsync(onNext, cancellationToken);

                if (count > 0)
                {
                    var average = this.divider(sum, count);

                    await observer.OnNextAsync(average, cancellationToken);

                    return null;
                }

                throw Error.EmptySequence();
            }
        }

        private sealed class AverageAsyncObservableNullable<TSource, TResult> : AsyncObservableBase<TResult?>
            where TResult : struct
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TResult?, TResult?, TResult?> adder;

            private readonly Func<TSource, TResult?> selector;

            private readonly Func<TResult?, long, TResult?> divider;

            public AverageAsyncObservableNullable(
                IAsyncObservable<TSource> source,
                Func<TResult?, TResult?, TResult?> adder,
                Func<TSource, TResult?> selector,
                Func<TResult?, long, TResult?> divider)
            {
                this.source = source;
                this.adder = adder;
                this.selector = selector;
                this.divider = divider;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult?> observer,
                CancellationToken cancellationToken)
            {
                var sum = default(TResult?);
                var count = 0L;

                Action<TSource> onNext = value =>
                {
                    var result = this.selector(value);
                    if (result.HasValue)
                    {
                        sum = this.adder(result, sum.GetValueOrDefault());
                        count++;
                    }
                };

                await source.ForEachAsync(onNext, cancellationToken);

                if (count > 0)
                {
                    var average = this.divider(sum, count);

                    await observer.OnNextAsync(average, cancellationToken);

                    return null;
                }

                throw Error.EmptySequence();
            }
        }

        private static double? ToDoubleNullable<T>(
            T? value)
            where T :struct
        {
            if (value.HasValue)
            {
                return Convert.ToDouble(value);
            }

            return null;
        }
    }
}

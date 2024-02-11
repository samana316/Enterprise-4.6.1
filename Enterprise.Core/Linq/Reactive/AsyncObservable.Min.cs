using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Min<TSource>(
            this IAsyncObservable<TSource> source)
        {
            return new MinAsyncObservable<TSource>(source, null);
        }

        public static IAsyncObservable<TSource> Min<TSource>(
            this IAsyncObservable<TSource> source,
            IComparer<TSource> comparer)
        {
            return new MinAsyncObservable<TSource>(source, comparer);
        }

        public static IAsyncObservable<double> Min(
            this IAsyncObservable<double> source)
        {
            return new MinAsyncObservable<double>(source, null);
        }

        public static IAsyncObservable<float> Min(
            this IAsyncObservable<float> source)
        {
            return new MinAsyncObservable<float>(source, null);
        }

        public static IAsyncObservable<decimal> Min(
            this IAsyncObservable<decimal> source)
        {
            return new MinAsyncObservable<decimal>(source, null);
        }

        public static IAsyncObservable<int> Min(
            this IAsyncObservable<int> source)
        {
            return new MinAsyncObservable<int>(source, null);
        }

        public static IAsyncObservable<long> Min(
            this IAsyncObservable<long> source)
        {
            return new MinAsyncObservable<long>(source, null);
        }

        public static IAsyncObservable<double?> Min(
            this IAsyncObservable<double?> source)
        {
            return new MinAsyncObservableNullable<double>(source, null);
        }

        public static IAsyncObservable<float?> Min(
            this IAsyncObservable<float?> source)
        {
            return new MinAsyncObservableNullable<float>(source, null);
        }

        public static IAsyncObservable<decimal?> Min(
            this IAsyncObservable<decimal?> source)
        {
            return new MinAsyncObservableNullable<decimal>(source, null);
        }

        public static IAsyncObservable<int?> Min(
            this IAsyncObservable<int?> source)
        {
            return new MinAsyncObservableNullable<int>(source, null);
        }

        public static IAsyncObservable<long?> Min(
            this IAsyncObservable<long?> source)
        {
            return new MinAsyncObservableNullable<long>(source, null);
        }

        public static IAsyncObservable<TResult> Min<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return new MinAsyncObservable<TResult>(source.Select(selector), null);
        }

        public static IAsyncObservable<TResult> Min<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TResult> selector,
            IComparer<TResult> comparer)
        {
            return new MinAsyncObservable<TResult>(source.Select(selector), comparer);
        }

        public static IAsyncObservable<double> Min<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double> selector)
        {
            return new MinAsyncObservable<double>(source.Select(selector), null);
        }

        public static IAsyncObservable<float> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, float> selector)
        {
            return new MinAsyncObservable<float>(source.Select(selector), null);
        }

        public static IAsyncObservable<decimal> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, decimal> selector)
        {
            return new MinAsyncObservable<decimal>(source.Select(selector), null);
        }

        public static IAsyncObservable<int> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, int> selector)
        {
            return new MinAsyncObservable<int>(source.Select(selector), null);
        }

        public static IAsyncObservable<long> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, long> selector)
        {
            return new MinAsyncObservable<long>(source.Select(selector), null);
        }

        public static IAsyncObservable<double?> Min<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double?> selector)
        {
            return new MinAsyncObservableNullable<double>(source.Select(selector), null);
        }

        public static IAsyncObservable<float?> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, float?> selector)
        {
            return new MinAsyncObservableNullable<float>(source.Select(selector), null);
        }

        public static IAsyncObservable<decimal?> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, decimal?> selector)
        {
            return new MinAsyncObservableNullable<decimal>(source.Select(selector), null);
        }

        public static IAsyncObservable<int?> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, int?> selector)
        {
            return new MinAsyncObservableNullable<int>(source.Select(selector), null);
        }

        public static IAsyncObservable<long?> Min<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, long?> selector)
        {
            return new MinAsyncObservableNullable<long>(source.Select(selector), null);
        }

        private sealed class MinAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly IComparer<TSource> comparer;

            public MinAsyncObservable(
                IAsyncObservable<TSource> source,
                IComparer<TSource> comparer)
            {
                Check.NotNull(source, "source");

                this.source = source;
                this.comparer = comparer ?? Comparer<TSource>.Default;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var hasValue = false;
                var lastValue = default(TSource);

                Action<TSource> onNext = value =>
                {
                    if (hasValue)
                    {
                        var num = 0;
                        try
                        {
                            num = this.comparer.Compare(value, lastValue);
                        }
                        catch (Exception error)
                        {
                            observer.OnError(error);
                            return;
                        }
                        if (num < 0)
                        {
                            lastValue = value;
                            return;
                        }
                    }
                    else
                    {
                        hasValue = true;
                        lastValue = value;
                    }
                };

                await this.source.ForEachAsync(onNext, cancellationToken);
                await observer.OnNextAsync(lastValue, cancellationToken);

                return null;
            }
        }

        private sealed class MinAsyncObservableNullable<TSource> : AsyncObservableBase<TSource?>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource?> source;

            private readonly IComparer<TSource> comparer;

            public MinAsyncObservableNullable(
                IAsyncObservable<TSource?> source,
                IComparer<TSource> comparer)
            {
                Check.NotNull(source, "source");

                this.source = source;
                this.comparer = comparer ?? Comparer<TSource>.Default;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource?> observer,
                CancellationToken cancellationToken)
            {
                var lastValue = default(TSource?);

                Action<TSource?> onNext = value =>
                {
                    if (!value.HasValue)
                    {
                        return;
                    }
                    if (lastValue.HasValue)
                    {
                        var num = this.comparer.Compare(value.Value, lastValue.Value);
                        if (num < 0)
                        {
                            lastValue = value;
                            return;
                        }
                    }
                    else
                    {
                        lastValue = value;
                    }
                };

                await this.source.ForEachAsync(onNext, cancellationToken);
                await observer.OnNextAsync(lastValue, cancellationToken);

                return null;
            }
        }
    }
}

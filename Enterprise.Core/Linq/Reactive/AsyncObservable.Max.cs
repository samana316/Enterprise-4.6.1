using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Max<TSource>(
            this IAsyncObservable<TSource> source)
        {
            return new MaxAsyncObservable<TSource>(source, null);
        }

        public static IAsyncObservable<TSource> Max<TSource>(
            this IAsyncObservable<TSource> source,
            IComparer<TSource> comparer)
        {
            return new MaxAsyncObservable<TSource>(source, comparer);
        }

        public static IAsyncObservable<double> Max(
            this IAsyncObservable<double> source)
        {
            return new MaxAsyncObservable<double>(source, null);
        }

        public static IAsyncObservable<float> Max(
            this IAsyncObservable<float> source)
        {
            return new MaxAsyncObservable<float>(source, null);
        }

        public static IAsyncObservable<decimal> Max(
            this IAsyncObservable<decimal> source)
        {
            return new MaxAsyncObservable<decimal>(source, null);
        }

        public static IAsyncObservable<int> Max(
            this IAsyncObservable<int> source)
        {
            return new MaxAsyncObservable<int>(source, null);
        }

        public static IAsyncObservable<long> Max(
            this IAsyncObservable<long> source)
        {
            return new MaxAsyncObservable<long>(source, null);
        }

        public static IAsyncObservable<double?> Max(
            this IAsyncObservable<double?> source)
        {
            return new MaxAsyncObservableNullable<double>(source, null);
        }

        public static IAsyncObservable<float?> Max(
            this IAsyncObservable<float?> source)
        {
            return new MaxAsyncObservableNullable<float>(source, null);
        }

        public static IAsyncObservable<decimal?> Max(
            this IAsyncObservable<decimal?> source)
        {
            return new MaxAsyncObservableNullable<decimal>(source, null);
        }

        public static IAsyncObservable<int?> Max(
            this IAsyncObservable<int?> source)
        {
            return new MaxAsyncObservableNullable<int>(source, null);
        }

        public static IAsyncObservable<long?> Max(
            this IAsyncObservable<long?> source)
        {
            return new MaxAsyncObservableNullable<long>(source, null);
        }

        public static IAsyncObservable<TResult> Max<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return new MaxAsyncObservable<TResult>(source.Select(selector), null);
        }

        public static IAsyncObservable<TResult> Max<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TResult> selector, 
            IComparer<TResult> comparer)
        {
            return new MaxAsyncObservable<TResult>(source.Select(selector), comparer);
        }

        public static IAsyncObservable<double> Max<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double> selector)
        {
            return new MaxAsyncObservable<double>(source.Select(selector), null);
        }

        public static IAsyncObservable<float> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, float> selector)
        {
            return new MaxAsyncObservable<float>(source.Select(selector), null);
        }

        public static IAsyncObservable<decimal> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, decimal> selector)
        {
            return new MaxAsyncObservable<decimal>(source.Select(selector), null);
        }

        public static IAsyncObservable<int> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, int> selector)
        {
            return new MaxAsyncObservable<int>(source.Select(selector), null);
        }

        public static IAsyncObservable<long> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, long> selector)
        {
            return new MaxAsyncObservable<long>(source.Select(selector), null);
        }

        public static IAsyncObservable<double?> Max<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, double?> selector)
        {
            return new MaxAsyncObservableNullable<double>(source.Select(selector), null);
        }

        public static IAsyncObservable<float?> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, float?> selector)
        {
            return new MaxAsyncObservableNullable<float>(source.Select(selector), null);
        }

        public static IAsyncObservable<decimal?> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, decimal?> selector)
        {
            return new MaxAsyncObservableNullable<decimal>(source.Select(selector), null);
        }

        public static IAsyncObservable<int?> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, int?> selector)
        {
            return new MaxAsyncObservableNullable<int>(source.Select(selector), null);
        }

        public static IAsyncObservable<long?> Max<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, long?> selector)
        {
            return new MaxAsyncObservableNullable<long>(source.Select(selector), null);
        }

        private sealed class MaxAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly IComparer<TSource> comparer;

            public MaxAsyncObservable(
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
                var lastValue = default(TSource);

                Action<TSource> onNext = value =>
                {
                    if (value != null)
                    {
                        if (lastValue == null)
                        {
                            lastValue = value;
                            return;
                        }
                        int num = 0;
                        try
                        {
                            num = this.comparer.Compare(value, lastValue);
                        }
                        catch (Exception error)
                        {
                            observer.OnError(error);
                            return;
                        }
                        if (num > 0)
                        {
                            lastValue = value;
                        }
                    }
                };

                await this.source.ForEachAsync(onNext, cancellationToken);
                await observer.OnNextAsync(lastValue, cancellationToken);

                return null;
            }
        }

        private sealed class MaxAsyncObservableNullable<TSource> : AsyncObservableBase<TSource?>
            where TSource : struct
        {
            private readonly IAsyncObservable<TSource?> source;

            private readonly IComparer<TSource> comparer;

            public MaxAsyncObservableNullable(
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
                        if (num > 0)
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

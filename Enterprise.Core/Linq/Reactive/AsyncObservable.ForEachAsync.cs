using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Action<TSource> onNext)
        {
            return source.ForEachAsync(onNext, CancellationToken.None);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Action<TSource> onNext,
            CancellationToken cancellationToken)
        {
            Func<TSource, int, CancellationToken, Task> overload =
                (value, index, ct) => { onNext(value); return TaskHelpers.Empty(); };

            return source.ForEachAsync(overload, cancellationToken);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Action<TSource, int> onNext)
        {
            return source.ForEachAsync(onNext, CancellationToken.None);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Action<TSource, int> onNext,
            CancellationToken cancellationToken)
        {
            Func<TSource, int, CancellationToken, Task> overload =
                (value, index, ct) => { onNext(value, index); return TaskHelpers.Empty(); };

            return source.ForEachAsync(overload, cancellationToken);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, CancellationToken, Task> onNextAsync)
        {
            return source.ForEachAsync(onNextAsync, CancellationToken.None);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, CancellationToken, Task> onNextAsync,
            CancellationToken cancellationToken)
        {
            Func<TSource, int, CancellationToken, Task> overload = 
                (value, index, ct) => onNextAsync(value, ct);

            return source.ForEachAsync(overload, cancellationToken);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, CancellationToken, Task> onNextAsync)
        {
            return source.ForEachAsync(onNextAsync, CancellationToken.None);
        }

        public static Task ForEachAsync<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, CancellationToken, Task> onNextAsync,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(source, "source");
            Check.NotNull(onNextAsync, "onNextAsync");

            var observer = new ForEachAsyncObserver<TSource>(onNextAsync);

            return source.SubscribeAsync(observer, cancellationToken);
        }

        private sealed class ForEachAsyncObserver<TSource> : AsyncObserverBase<TSource>
        {
            private readonly Func<TSource, int, CancellationToken, Task> onNextAsync;

            private int index;

            public ForEachAsyncObserver(
                Func<TSource, int, CancellationToken, Task> onNextAsync)
            {
                this.onNextAsync = onNextAsync;
            }

            protected override void OnCompletedCore()
            {
            }

            protected override void OnErrorCore(
                Exception error)
            {
            }

            protected override Task OnNextCoreAsync(
                TSource value, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return this.onNextAsync(value, this.index++, cancellationToken);
            }
        }
    }
}

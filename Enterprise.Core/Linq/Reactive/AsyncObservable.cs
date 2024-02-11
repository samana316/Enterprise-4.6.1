using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    public static partial class AsyncObservable
    {
        public static Task<IDisposable> SubscribeAsync<TSource>(
            this IAsyncObservable<TSource> source,
            IAsyncObserver<TSource> observer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            return source.SubscribeAsync(observer, CancellationToken.None);
        }

        public static Task<IDisposable> SubscribeAsync<TSource>(
            this IObservable<TSource> source,
            IAsyncObserver<TSource> observer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            return source.SubscribeAsync(observer, CancellationToken.None);
        }

        public static Task<IDisposable> SubscribeAsync<TSource>(
            this IObservable<TSource> source,
            IAsyncObserver<TSource> observer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            return source.AsAsyncObservable().SubscribeAsync(observer, cancellationToken);
        }

        public static Task<IDisposable> SubscribeSafeAsync<TSource>(
            this IAsyncObservable<TSource> source,
            IAsyncObserver<TSource> observer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            return source.SubscribeSafeAsync(observer, CancellationToken.None);
        }

        public static Task<IDisposable> SubscribeSafeAsync<TSource>(
            this IAsyncObservable<TSource> source,
            IAsyncObserver<TSource> observer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            if (source is AsyncObservableBase<TSource>)
            {
                return source.SubscribeAsync(observer, cancellationToken);
            }

            Func<IAsyncObserver<TSource>, CancellationToken, Task<IDisposable>> subscribeAsync =
                source.SubscribeAsync;

            return Create(subscribeAsync).SubscribeAsync(observer, cancellationToken);
        }

        public static Task<IDisposable> SubscribeRawAsync<TSource>(
            this IAsyncObservable<TSource> source,
            IAsyncObserver<TSource> observer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            return source.SubscribeRawAsync(observer, CancellationToken.None);
        }

        public static Task<IDisposable> SubscribeRawAsync<TSource>(
            this IAsyncObservable<TSource> source,
            IAsyncObserver<TSource> observer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(source, "source");
            Check.NotNull(observer, "observer");

            var raw = source as AsyncObservableBase<TSource>;
            if (!ReferenceEquals(raw, null))
            {
                return raw.SubscribeRawAsync(observer, cancellationToken);
            }

            return source.SubscribeAsync(observer, cancellationToken);
        }

        private static TElement IdentityFunction<TElement>(
            TElement element)
        {
            return element;
        }
    }
}

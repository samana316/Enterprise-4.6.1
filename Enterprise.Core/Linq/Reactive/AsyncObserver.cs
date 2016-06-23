using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    public static partial class AsyncObserver
    {
        public static Task OnNextAsync<TSource>(
            this IAsyncObserver<TSource> observer,
            TSource value)
        {
            Check.NotNull(observer, "observer");

            return observer.OnNextAsync(value, CancellationToken.None);
        }

        public static Task OnNextAsync<TSource>(
            this IObserver<TSource> observer,
            TSource value)
        {
            Check.NotNull(observer, "observer");

            return observer.OnNextAsync(value, CancellationToken.None);
        }

        public static Task OnNextAsync<TSource>(
            this IObserver<TSource> observer,
            TSource value,
            CancellationToken cancellationToken)
        {
            Check.NotNull(observer, "observer");

            return observer.AsAsyncObserver().OnNextAsync(value, cancellationToken);
        }
    }
}

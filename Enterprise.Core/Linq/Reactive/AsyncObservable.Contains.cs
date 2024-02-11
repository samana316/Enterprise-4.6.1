using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<bool> Contains<TSource>(
            this IAsyncObservable<TSource> source,
            TSource value)
        {
            Check.NotNull(source, "source");

            return new ContainsAsyncObservable<TSource>(source, value, null);
        }

        public static IAsyncObservable<bool> Contains<TSource>(
            this IAsyncObservable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(comparer, "comparer");

            return new ContainsAsyncObservable<TSource>(source, value, comparer);
        }

        private sealed class ContainsAsyncObservable<TSource> : AsyncObservableBase<bool>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly TSource value;

            private readonly IEqualityComparer<TSource> comparer;

            public ContainsAsyncObservable(
                IAsyncObservable<TSource> source,
                TSource value,
                IEqualityComparer<TSource> comparer)
            {
                this.source = source;
                this.value = value;
                this.comparer = comparer ?? EqualityComparer<TSource>.Default;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<bool> observer,
                CancellationToken cancellationToken)
            {
                var query = this.source.Any(this.Match);

                return query.SubscribeSafeAsync(observer, cancellationToken);
            }

            private bool Match(
                TSource item)
            {
                return this.comparer.Equals(item, this.value);
            }
        }
    }
}

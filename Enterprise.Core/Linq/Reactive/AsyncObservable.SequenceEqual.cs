using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<bool> SequenceEqual<TSource>(
            this IAsyncObservable<TSource> first,
            IObservable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new SequenceEqualAsyncObservable<TSource>(first, second, null);
        }

        public static IAsyncObservable<bool> SequenceEqual<TSource>(
            this IAsyncObservable<TSource> first,
            IObservable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");
            Check.NotNull(comparer, "comparer");

            return new SequenceEqualAsyncObservable<TSource>(first, second, comparer);
        }

        public static IAsyncObservable<bool> SequenceEqual<TSource>(
            this IAsyncObservable<TSource> first,
            IEnumerable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new SequenceEqualAsyncObservable<TSource>(first, second, null);
        }

        public static IAsyncObservable<bool> SequenceEqual<TSource>(
            this IAsyncObservable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");
            Check.NotNull(comparer, "comparer");

            return new SequenceEqualAsyncObservable<TSource>(first, second, comparer);
        }

        private sealed class SequenceEqualAsyncObservable<TSource> : AsyncObservableBase<bool>
        {
            private readonly IAsyncObservable<TSource> first;

            private readonly IAsyncObservable<TSource> second;

            private readonly IAsyncEnumerable<TSource> secondE;

            private readonly IEqualityComparer<TSource> comparer;

            public SequenceEqualAsyncObservable(
                IAsyncObservable<TSource> first,
                IObservable<TSource> second,
                IEqualityComparer<TSource> comparer)
            {
                this.first = first;
                this.second = second.AsAsyncObservable();
                this.comparer = comparer ?? EqualityComparer<TSource>.Default;
            }

            public SequenceEqualAsyncObservable(
                IAsyncObservable<TSource> first,  
                IEnumerable<TSource> second, 
                IEqualityComparer<TSource> comparer)
            {
                this.first = first;
                this.secondE = second.AsAsyncEnumerable();
                this.comparer = comparer ?? EqualityComparer<TSource>.Default;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<bool> observer, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (this.second != null)
                {
                    return this.ObservableImplAsync(observer, cancellationToken);
                }

                if (this.secondE != null)
                {
                    return this.EnumerableImplAsync(observer, cancellationToken);
                }

                throw new InvalidOperationException();
            }

            private async Task<IDisposable> ObservableImplAsync(
                IAsyncObserver<bool> observer,
                CancellationToken cancellationToken)
            {
                var list = new List<TSource>();
                await second.ForEachAsync(list.Add, cancellationToken);

                var result = first.SequenceEqual(list, this.comparer);

                return await result.SubscribeSafeAsync(observer, cancellationToken);
            }

            private async Task<IDisposable> EnumerableImplAsync(
                IAsyncObserver<bool> observer, 
                CancellationToken cancellationToken)
            {
                using (var enumerator = secondE.GetAsyncEnumerator())
                {
                    var count = 0;
                    var result = false;

                    Func<TSource, CancellationToken, Task> onNextAsync = async (item, ct) =>
                    {
                        ct.ThrowIfCancellationRequested();

                        var condition = (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
                            && comparer.Equals(enumerator.Current, item);

                        result = count == 0 ? condition : result && condition;
                        count++;
                    };

                    await first.ForEachAsync(onNextAsync, cancellationToken);
                    await observer.OnNextAsync(result, cancellationToken);
                }

                return null;
            }
        }
    }
}

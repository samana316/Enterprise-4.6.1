using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static Task<bool> SequenceEqualAsync<TSource>(
           this IAsyncEnumerable<TSource> first,
           IEnumerable<TSource> second)
        {
            return first.SequenceEqualAsync(second, CancellationToken.None);
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
           this IAsyncEnumerable<TSource> first,
           IEnumerable<TSource> second,
           CancellationToken cancellationToken)
        {
            return first.SequenceEqualAsync(second, EqualityComparer<TSource>.Default);
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
            this IAsyncEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            return first.SequenceEqualAsync(second, comparer, CancellationToken.None);
        }

        public static async Task<bool> SequenceEqualAsync<TSource>(
            this IAsyncEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            if (ReferenceEquals(comparer, null))
            {
                comparer = EqualityComparer<TSource>.Default;
            }

            using (var enumerator = first.GetAsyncEnumerator())
            {
                using (var enumerator2 = second.GetEnumerator())
                {
                    while (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        if (!(await enumerator2.MoveNextAsync(cancellationToken).ConfigureAwait(false)) || 
                            !comparer.Equals(enumerator.Current, enumerator2.Current))
                        {
                            return false;
                        }
                    }

                    if (await enumerator2.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

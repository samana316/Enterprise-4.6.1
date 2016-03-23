using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return new AsyncOrderedEnumerable<TSource, TKey>(source, keySelector, null, false);
        }

        public static IAsyncOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey> comparer)
        {
            return new AsyncOrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public static IAsyncOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return new AsyncOrderedEnumerable<TSource, TKey>(source, keySelector, null, true);
        }

        public static IAsyncOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey> comparer)
        {
            return new AsyncOrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public static IAsyncEnumerable<TSource> Reverse<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            Check.NotNull(source, "source");

            return CreateBufferred<TSource>((yielder, cancellationToken) =>
                ReverseAsyncIterator(source, yielder, cancellationToken));
        }

        public static IAsyncOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IAsyncOrderedEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");

            return source.CreateAsyncOrderedEnumerable(keySelector, null, false);
        }

        public static IAsyncOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IAsyncOrderedEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");

            return source.CreateAsyncOrderedEnumerable(keySelector, comparer, false);
        }

        public static IAsyncOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IAsyncOrderedEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");

            return source.CreateAsyncOrderedEnumerable(keySelector, null, true);
        }

        public static IAsyncOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IAsyncOrderedEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");

            return source.CreateAsyncOrderedEnumerable(keySelector, comparer, true);
        }

        private static async Task ReverseAsyncIterator<TSource>(
            IAsyncEnumerable<TSource> source,
            IAsyncYielder<TSource> yielder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buffer = await source.ToArrayAsync(cancellationToken);

            int num;
            for (int i = buffer.Length - 1; i >= 0; i = num - 1)
            {
                await yielder.ReturnAsync(buffer[i], cancellationToken);
                num = i;
            }
            await yielder.BreakAsync(cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, null);
        }

        public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, comparer);
        }

        public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, null);
        }

        public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TElement, TResult>(
                source, keySelector, elementSelector, resultSelector, null);
        }

        public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new AsyncGroupedEnumerable<TSource, TKey, TElement, TResult>(
                source, keySelector, elementSelector, resultSelector, comparer);
        }
    }
}

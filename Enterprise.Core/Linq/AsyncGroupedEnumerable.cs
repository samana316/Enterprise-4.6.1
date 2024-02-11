using System;
using System.Collections;
using System.Collections.Generic;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    internal class AsyncGroupedEnumerable<TSource, TKey, TElement> :
        IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
    {
        private IAsyncEnumerable<TSource> source;

        private Func<TSource, TKey> keySelector;

        private Func<TSource, TElement> elementSelector;

        private IEqualityComparer<TKey> comparer;

        public AsyncGroupedEnumerable(
            IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
        }

        public IAsyncEnumerator<IAsyncGrouping<TKey, TElement>> GetAsyncEnumerator()
        {
            return AsyncLookup<TKey, TElement>.Create(
                this.source, this.keySelector, this.elementSelector, this.comparer)
                .GetAsyncEnumerator();
        }

        public IEnumerator<IAsyncGrouping<TKey, TElement>> GetEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetAsyncEnumerator();
        }
    }

    internal class AsyncGroupedEnumerable<TSource, TKey, TElement, TResult> :
        IAsyncEnumerable<TResult>
    {
        private IAsyncEnumerable<TSource> source;

        private Func<TSource, TKey> keySelector;

        private Func<TSource, TElement> elementSelector;

        private IEqualityComparer<TKey> comparer;

        private Func<TKey, IEnumerable<TElement>, TResult> resultSelector;

        public AsyncGroupedEnumerable(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");
            Check.NotNull(resultSelector, "resultSelector");

            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
            this.resultSelector = resultSelector;
        }

        public IAsyncEnumerator<TResult> GetAsyncEnumerator()
        {
            return AsyncLookup<TKey, TElement>.Create(
                this.source,
                this.keySelector,
                this.elementSelector,
                this.comparer)
                .ApplyResultSelector(this.resultSelector)
                .GetAsyncEnumerator();
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetAsyncEnumerator();
        }
    }
}

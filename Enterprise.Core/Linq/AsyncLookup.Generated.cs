using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    internal partial class AsyncLookup<TKey, TElement> : IAsyncLookup<TKey, TElement>
    {
        private IEqualityComparer<TKey> comparer;

        private AsyncGrouping[] groupings;

        private AsyncGrouping lastGrouping;

        private int count;

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                AsyncGrouping grouping = this.GetGrouping(key, false);
                if (grouping != null)
                {
                    return grouping;
                }
                return Enumerable.Empty<TElement>();
            }
        }

        internal static AsyncLookup<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");
            
            var lookup = new AsyncLookup<TKey, TElement>(comparer);
            foreach (TSource current in source)
            {
                lookup.GetGrouping(keySelector(current), true).Add(elementSelector(current));
            }
            return lookup;
        }

        internal static AsyncLookup<TKey, TElement> CreateForJoin(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var lookup = new AsyncLookup<TKey, TElement>(comparer);
            foreach (TElement current in source)
            {
                TKey tKey = keySelector(current);
                if (tKey != null)
                {
                    lookup.GetGrouping(tKey, true).Add(current);
                }
            }
            return lookup;
        }

        private AsyncLookup(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }
            this.comparer = comparer;
            this.groupings = new AsyncGrouping[7];
        }

        public bool Contains(TKey key)
        {
            return this.GetGrouping(key, false) != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            AsyncGrouping next = this.lastGrouping;
            if (next != null)
            {
                do
                {
                    next = next.next;
                    yield return next;
                }
                while (next != this.lastGrouping);
            }
            yield break;
        }

        //public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        //{
        //    AsyncGrouping next = this.lastGrouping;
        //    if (next != null)
        //    {
        //        do
        //        {
        //            next = next.next;
        //            if (next.count != next.elements.Length)
        //            {
        //                Array.Resize<TElement>(ref next.elements, next.count);
        //            }
        //            yield return resultSelector(next.key, next.elements);
        //        }
        //        while (next != this.lastGrouping);
        //    }
        //    yield break;
        //}

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal int InternalGetHashCode(TKey key)
        {
            if (key != null)
            {
                return this.comparer.GetHashCode(key) & 2147483647;
            }
            return 0;
        }

        internal AsyncGrouping GetGrouping(TKey key, bool create)
        {
            int num = this.InternalGetHashCode(key);
            for (AsyncGrouping grouping = this.groupings[num % this.groupings.Length]; grouping != null; grouping = grouping.hashNext)
            {
                if (grouping.hashCode == num && this.comparer.Equals(grouping.key, key))
                {
                    return grouping;
                }
            }
            if (create)
            {
                if (this.count == this.groupings.Length)
                {
                    this.Resize();
                }
                int num2 = num % this.groupings.Length;
                AsyncGrouping grouping2 = new AsyncGrouping();
                grouping2.key = key;
                grouping2.hashCode = num;
                grouping2.elements = new TElement[1];
                grouping2.hashNext = this.groupings[num2];
                this.groupings[num2] = grouping2;
                if (this.lastGrouping == null)
                {
                    AsyncGrouping expr_AC = grouping2;
                    expr_AC.next = expr_AC;
                }
                else
                {
                    grouping2.next = this.lastGrouping.next;
                    this.lastGrouping.next = grouping2;
                }
                this.lastGrouping = grouping2;
                this.count++;
                return grouping2;
            }
            return null;
        }

        private void Resize()
        {
            int num = checked(this.count * 2 + 1);
            AsyncGrouping[] array = new AsyncGrouping[num];
            AsyncGrouping next = this.lastGrouping;
            do
            {
                next = next.next;
                int num2 = next.hashCode % num;
                next.hashNext = array[num2];
                array[num2] = next;
            }
            while (next != this.lastGrouping);
            this.groupings = array;
        }
    }
}

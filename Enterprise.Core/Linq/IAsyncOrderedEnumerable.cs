using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Core.Linq
{
    public interface IAsyncOrderedEnumerable<TElement> : 
        IAsyncEnumerable<TElement>, 
        IOrderedEnumerable<TElement>
    {
        IAsyncOrderedEnumerable<TElement> CreateAsyncOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}

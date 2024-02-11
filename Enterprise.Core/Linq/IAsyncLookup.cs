using System.Linq;
using System.Collections.Generic;

namespace Enterprise.Core.Linq
{
    public interface IAsyncLookup<TKey, TElement> :
        ILookup<TKey, TElement>,
        IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>,
        IReadOnlyCollection<IAsyncGrouping<TKey, TElement>>
    {
        new IAsyncEnumerable<TElement> this[TKey key] { get; }
    }
}

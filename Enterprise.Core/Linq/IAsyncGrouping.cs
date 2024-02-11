using System.Linq;

namespace Enterprise.Core.Linq
{
    public interface IAsyncGrouping<out TKey, out TElement> : 
        IGrouping<TKey, TElement>,
        IAsyncEnumerable<TElement>
    {
    }
}

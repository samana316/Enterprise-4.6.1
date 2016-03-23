using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Core.Linq
{
    public interface IAsyncEnumerable : IEnumerable
    {
        IAsyncEnumerator GetAsyncEnumerator();
    }

    public interface IAsyncEnumerable<out T> : IAsyncEnumerable, IEnumerable<T>
    {
        new IAsyncEnumerator<T> GetAsyncEnumerator();
    }
}

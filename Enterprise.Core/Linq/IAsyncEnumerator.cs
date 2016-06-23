using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    public interface IAsyncEnumerator : IEnumerator, IDisposable
    {
        Task<bool> MoveNextAsync(CancellationToken cancellationToken);
    }

    public interface IAsyncEnumerator<out T> : IAsyncEnumerator, IEnumerator<T>
    {
    }
}

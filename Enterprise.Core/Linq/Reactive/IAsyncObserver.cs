using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    public interface IAsyncObserver<in T> : IObserver<T>
    {
        Task OnNextAsync(T value, CancellationToken cancellationToken);
    }
}

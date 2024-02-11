using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    public interface IAsyncObservable<out T> : IObservable<T>
    {
        Task<IDisposable> SubscribeAsync(IAsyncObserver<T> observer, CancellationToken cancellationToken);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    internal interface IProducer
    {
        Task<IDisposable> RunAsync(CancellationToken cancellationToken);
    }
}

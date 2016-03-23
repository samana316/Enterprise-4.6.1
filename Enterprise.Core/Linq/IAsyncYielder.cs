using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    public interface IAsyncYielder<TResult>
    {
        Task ReturnAsync(TResult value, CancellationToken cancellationToken);

        Task BreakAsync(CancellationToken cancellationToken);
    }
}

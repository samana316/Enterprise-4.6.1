using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Providers
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken);

        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    }
}

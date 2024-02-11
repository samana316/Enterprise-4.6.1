using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Providers
{
    public static class AsyncQueryProviderExtensions
    {
        public static Task<object> ExecuteAsync(
            IAsyncQueryProvider provider,
            Expression expression)
        {
            Check.NotNull(provider, "provider");

            return provider.ExecuteAsync(expression, CancellationToken.None);
        }

        public static Task<TResult> ExecuteAsync<TResult>(
            IAsyncQueryProvider provider,
            Expression expression)
        {
            Check.NotNull(provider, "provider");

            return provider.ExecuteAsync<TResult>(expression, CancellationToken.None);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    public static class AsyncYielder
    {
        public static Task ReturnAsync<TResult>(
            this IAsyncYielder<TResult> yielder,
            TResult value)
        {
            Check.NotNull(yielder, "yielder");

            return yielder.ReturnAsync(value, CancellationToken.None);
        }

        public static Task BreakAsync<TResult>(
            this IAsyncYielder<TResult> yielder)
        {
            Check.NotNull(yielder, "yielder");

            return yielder.BreakAsync(CancellationToken.None);
        }
    }
}

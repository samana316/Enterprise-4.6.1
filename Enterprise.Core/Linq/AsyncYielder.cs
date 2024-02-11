using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    public static class AsyncYielder
    {
        public static Task BreakAsync<TResult>(
            this IAsyncYielder<TResult> yielder)
        {
            Check.NotNull(yielder, "yielder");

            return yielder.BreakAsync(CancellationToken.None);
        }

        public static Task ReturnAllAsync<TResult>(
            this IAsyncYielder<TResult> yielder,
            IEnumerable<TResult> source)
        {
            Check.NotNull(yielder, "yielder");

            return yielder.ReturnAllAsync(source, CancellationToken.None);
        }

        public static Task ReturnAllAsync<TResult>(
            this IAsyncYielder<TResult> yielder,
            IEnumerable<TResult> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(yielder, "yielder");

            return source.AsAsyncEnumerable().ForEachAsync(
                async item => await yielder.ReturnAsync(item, cancellationToken));
        }

        public static Task ReturnAsync<TResult>(
            this IAsyncYielder<TResult> yielder,
            TResult value)
        {
            Check.NotNull(yielder, "yielder");

            return yielder.ReturnAsync(value, CancellationToken.None);
        }
    }
}

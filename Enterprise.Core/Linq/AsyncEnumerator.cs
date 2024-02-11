using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    public static class AsyncEnumerator
    {
        public static Task<bool> MoveNextAsync(
            this IAsyncEnumerator enumerator)
        {
            Check.NotNull(enumerator, "enumerator");

            return enumerator.MoveNextAsync(CancellationToken.None);
        }

        public static Task<bool> MoveNextAsync(
            this IEnumerator enumerator)
        {
            Check.NotNull(enumerator, "enumerator");

            return enumerator.MoveNextAsync(CancellationToken.None);
        }

        public static Task<bool> MoveNextAsync(
            this IEnumerator enumerator,
            CancellationToken cancellationToken)
        {
            Check.NotNull(enumerator, "enumerator");

            var asyncEnumerator = enumerator as IAsyncEnumerator;
            if (asyncEnumerator != null)
            {
                return asyncEnumerator.MoveNextAsync(cancellationToken);
            }

            return Task.Run(new Func<bool>(enumerator.MoveNext), cancellationToken);
        }
    }
}

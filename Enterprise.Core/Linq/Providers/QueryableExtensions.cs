using System.Linq;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Providers
{
    partial class QueryableExtensions
    {
        public static IQueryable<TElement> AsQueryable<TElement>(
            this IAsyncEnumerable<TElement> source)
        {
            Check.NotNull(source, "source");

            var query = source as IQueryable<TElement>;

            if (query != null && query.Provider is IAsyncQueryProvider)
            {
                return query;
            }

            return new AsyncEnumerableQuery<TElement>(source);
        }
    }
}

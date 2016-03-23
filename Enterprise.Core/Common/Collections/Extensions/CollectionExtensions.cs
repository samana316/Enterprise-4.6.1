using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Collections.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<TSource>(
            this ICollection<TSource> source,
            IEnumerable<TSource> collection)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collection, "collection");

            var list = source as List<TSource>;
            if (!ReferenceEquals(list, null))
            {
                list.AddRange(collection);
                return;
            }

            collection.ForEach(source.Add);
        }
    }
}

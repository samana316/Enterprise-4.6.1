using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Collections.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(
            this IEnumerable source)
        {
            if (ReferenceEquals(source, null))
            {
                return true;
            }

            var enumerator = source.GetEnumerator();

            return !enumerator.MoveNext();
        }

        public static bool IsNullOrEmpty<TSource>(
            this IEnumerable<TSource> source)
        {
            if (ReferenceEquals(source, null))
            {
                return true;
            }

            return !source.Any();
        }

        public static void ForEach(
            this IEnumerable source,
            Action<object> action)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            var enumerator = source.GetEnumerator();

            while (enumerator.MoveNext())
            {
                action(enumerator.Current);
            }
        }

        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> action)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}

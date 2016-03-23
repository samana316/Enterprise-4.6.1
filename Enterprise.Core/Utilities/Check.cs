using System;
using System.Collections.Generic;
using Enterprise.Core.Common.Collections.Extensions;

namespace Enterprise.Core.Utilities
{
    internal static class Check
    {
        public static T NotNull<T>(
            T value,
            string parameterName = "")
            where T : class
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static T? NotNull<T>(
            T? value,
            string parameterName = "")
            where T : struct
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static string NotEmpty(
            string value,
            string parameterName = "")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return value;
        }

        public static IEnumerable<T> NotNullOrEmpty<T>(
            IEnumerable<T> collection,
            string parameterName = "")
        {
            if (collection.IsNullOrEmpty())
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return collection;
        }
    }
}

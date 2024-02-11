using System;

namespace Enterprise.Core.Resources
{
    internal static class Error
    {
        internal static Exception EmptySequence()
        {
            return new InvalidOperationException("Sequence contains no elements");
        }

        internal static Exception NoMatch()
        {
            return new InvalidOperationException("Sequence contains no matching element");
        }

        internal static Exception MoreThanOneElement()
        {
            return new InvalidOperationException("Sequence contains more than one element");
        }

        [Obsolete("This method only exists to resolve compiler error.")]
        internal static Exception ArgumentNull(
            string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        internal static Exception ArgumentOutOfRange(
            string parameterName)
        {
            return new ArgumentOutOfRangeException(parameterName);
        }

        internal static Exception MoreThanOneMatch()
        {
            return new InvalidOperationException("Sequence contains more than one matching element");
        }

        internal static Exception IQueryable_Provider_Not_Async()
        {
            throw new NotSupportedException();
        }

        internal static Exception IQueryable_Not_Async(
            string empty)
        {
            throw new NotSupportedException();
        }

        internal static Exception NotSupported()
        {
            throw new NotSupportedException();
        }
    }
}

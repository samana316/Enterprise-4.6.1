using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Runtime.ExceptionServices
{
    public static class ExceptionExtensions
    {
        public static void Rethrow(
            this Exception exception)
        {
            Check.NotNull(exception, "exception");

            var aggregate = exception as AggregateException;
            if (aggregate != null && aggregate.InnerException != null)
            {
                exception = aggregate.InnerException;
            }

            var invocation = exception as TargetInvocationException;
            if (invocation != null && invocation.InnerException != null)
            {
                exception = invocation.InnerException;
            }

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}

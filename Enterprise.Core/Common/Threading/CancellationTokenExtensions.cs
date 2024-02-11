using System;
using System.Reflection;
using System.Threading;

namespace Enterprise.Core.Common.Threading
{
    internal static class CancellationTokenExtensions
    {
        public static CancellationTokenSource GetSource(
            this CancellationToken cancellationToken)
        {
            var fieldInfo = typeof(CancellationToken).GetField(
                "m_source", BindingFlags.NonPublic | BindingFlags.Instance);

            if (ReferenceEquals(fieldInfo, null))
            {
                throw new InvalidOperationException();
            }

            var cancellationTokenSource = fieldInfo.GetValue(cancellationToken) as CancellationTokenSource;

            return cancellationTokenSource;
        }
    }
}

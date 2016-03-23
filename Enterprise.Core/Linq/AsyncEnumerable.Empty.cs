using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Empty<TResult>()
        {
            return EmptyAsyncEnumerable<TResult>.Instance;
        }

        private class EmptyAsyncEnumerable<TElement> : AsyncIterator<TElement>
        {
            private static EmptyAsyncEnumerable<TElement> instance = new EmptyAsyncEnumerable<TElement>();

            private EmptyAsyncEnumerable()
            {
            }

            public static AsyncIterator<TElement> Instance
            {
                get { return instance; }
            }

            public override TElement Current
            {
                get { throw new InvalidOperationException(); }
            }

            public override AsyncIterator<TElement> Clone()
            {
                return this;
            }

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                return TaskHelpers.Constant(false);
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Resources;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Repeat<TResult>(
            TResult element,
            int count)
        {
            if (count < 0)
            {
                throw Error.ArgumentOutOfRange("count");
            }

            return new RepeatAsyncIterator<TResult>(element, count);
        }

        private class RepeatAsyncIterator<TResult> : AsyncIterator<TResult>
        {
            private readonly TResult element;

            private readonly int count;

            private int currentIndex = -1;

            public RepeatAsyncIterator(
                TResult element,
                int count)
            {
                this.element = element;
                this.count = count;
            }

            public override TResult Current
            {
                get { return this.element; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new RepeatAsyncIterator<TResult>(this.element, this.count);
            }

            public override void Reset()
            {
                this.currentIndex = -1;
            }

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                Func<bool> func = this.InternalMoveNext;

                return func.InvokeAsync(cancellationToken);
            }

            private bool InternalMoveNext()
            {
                this.currentIndex++;

                return this.currentIndex < this.count;
            }
        }
    }
}

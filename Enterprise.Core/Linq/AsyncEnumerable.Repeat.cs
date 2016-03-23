using System.Threading;
using System.Threading.Tasks;
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

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                this.currentIndex++;

                var taskCompletionSource = new TaskCompletionSource<bool>();
                taskCompletionSource.SetResult(this.currentIndex < this.count);

                return taskCompletionSource.Task;
            }
        }
    }
}

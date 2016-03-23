using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<int> Range(
            int start,
            int count)
        {
            var num = (long)start + count - 1L;

            if (count < 0 || num > 2147483647L)
            {
                throw Error.ArgumentOutOfRange("count");
            }

            return new RangeAsyncIterator(start, count);
        }

        private class RangeAsyncIterator : AsyncIterator<int>
        {
            private readonly int start;

            private readonly int count;

            private int currentIndex = -1;

            private int current;

            public RangeAsyncIterator(
                int start,
                int count)
            {
                this.start = start;
                this.count = count;
            }

            public override int Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<int> Clone()
            {
                return new RangeAsyncIterator(this.start, this.count);
            }

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                this.currentIndex++;
                this.current = this.start + this.currentIndex;

                var taskCompletionSource = new TaskCompletionSource<bool>();
                taskCompletionSource.SetResult(this.currentIndex < this.count);

                return taskCompletionSource.Task;
            }
        }
    }
}

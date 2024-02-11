using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> Concat<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new ConcatAsyncIterator<TSource>(first, second);
        }

        private class ConcatAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            private readonly IAsyncEnumerable<TSource> first;

            private readonly IEnumerable<TSource> second;

            private IAsyncEnumerator<TSource> firstEnumerator;

            private IEnumerator<TSource> secondEnumerator;

            private TSource current;

            public ConcatAsyncIterator(
                IAsyncEnumerable<TSource> first,
                IEnumerable<TSource> second)
            {
                this.first = first;
                this.second = second;
            }

            public override TSource Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new ConcatAsyncIterator<TSource>(this.first, this.second);
            }

            public override void Reset()
            {
                this.Dispose();
                this.firstEnumerator = null;
                this.secondEnumerator = null;

                base.Reset();
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (this.firstEnumerator != null)
                    {
                        this.firstEnumerator.Dispose();
                    }

                    if (this.secondEnumerator != null)
                    {
                        this.secondEnumerator.Dispose();
                    }
                }

                base.Dispose(disposing);
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.firstEnumerator == null)
                {
                    this.firstEnumerator = this.first.GetAsyncEnumerator();
                }

                if (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.firstEnumerator.Current;
                    return true;
                }

                if (this.secondEnumerator == null)
                {
                    this.secondEnumerator = this.second.GetEnumerator();
                }

                if (await this.secondEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.secondEnumerator.Current;
                    return true;
                }

                return false;
            }
        }
    }
}

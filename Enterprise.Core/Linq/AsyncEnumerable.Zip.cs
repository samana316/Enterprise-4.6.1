using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IAsyncEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");
            Check.NotNull(resultSelector, "resultSelector");

            return new ZipAsyncIterator<TFirst, TSecond, TResult>(first, second, resultSelector);
        }

        private sealed class ZipAsyncIterator<TFirst, TSecond, TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable<TFirst> first;

            private readonly IEnumerable<TSecond> second;

            private readonly Func<TFirst, TSecond, TResult> resultSelector;

            private IAsyncEnumerator<TFirst> firstEnumerator;

            private IEnumerator<TSecond> secondEnumerator;

            private TResult current;

            public ZipAsyncIterator(
                IAsyncEnumerable<TFirst> first,
                IEnumerable<TSecond> second,
                Func<TFirst, TSecond, TResult> resultSelector)
            {
                this.first = first;
                this.second = second;
                this.resultSelector = resultSelector;
            }

            public override TResult Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new ZipAsyncIterator<TFirst, TSecond, TResult>(
                    this.first, this.second, this.resultSelector);
            }

            protected override void Dispose(
                bool disposing)
            {
                if (this.firstEnumerator != null)
                {
                    this.firstEnumerator.Dispose();
                    this.firstEnumerator = null;
                }

                if (this.secondEnumerator != null)
                {
                    this.secondEnumerator.Dispose();
                    this.secondEnumerator = null;
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

                if (this.secondEnumerator == null)
                {
                    this.secondEnumerator = this.second.GetEnumerator();
                }

                if (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false) &&
                    await this.secondEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.resultSelector(
                        this.firstEnumerator.Current,
                        this.secondEnumerator.Current);

                    return true;
                }

                return false;
            }
        }
    }
}

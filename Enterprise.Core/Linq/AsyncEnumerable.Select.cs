using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
           this IAsyncEnumerable<TSource> source,
           Func<TSource, TResult> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, int, TResult> overload = (item, index) => selector(item);

            return new SelectAsyncIterator<TSource, TResult>(source, overload);
        }

        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, int, TResult> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SelectAsyncIterator<TSource, TResult>(source, selector);
        }

        private class SelectAsyncIterator<TSource, TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable<TSource> source;

            private readonly Func<TSource, int, TResult> selector;

            private int currentIndex = -1;

            private TResult current;

            private IAsyncEnumerator<TSource> sourceEnumerator;

            public SelectAsyncIterator(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, TResult> selector)
            {
                this.source = source;
                this.selector = selector;
            }

            public override TResult Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new SelectAsyncIterator<TSource, TResult>(this.source, this.selector);
            }

            public override void Reset()
            {
                this.Dispose();
                this.sourceEnumerator = null;
                base.Reset();
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.sourceEnumerator == null)
                {
                    this.sourceEnumerator = this.source.GetAsyncEnumerator();
                }

                if (await this.sourceEnumerator.MoveNextAsync(cancellationToken))
                {
                    checked { this.currentIndex++; };
                    this.current = this.selector(this.sourceEnumerator.Current, this.currentIndex);

                    return true;
                }

                return false;
            }

            protected override void Dispose(
               bool disposing)
            {
                if (disposing)
                {
                    if (this.sourceEnumerator != null)
                    {
                        this.sourceEnumerator.Dispose();
                    }
                }

                base.Dispose(disposing);
            }
        }
    }
}

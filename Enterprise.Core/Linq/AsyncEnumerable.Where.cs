using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> Where<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            Func<TSource, int, bool> overload = (item, index) => predicate(item);

            //return new WhereAsyncIterator<TSource>(source, overload);

            return Create<TSource>(async (yielder, cancellationToken) =>
            {
                using (var enumerator = source.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        if (predicate(enumerator.Current))
                        {
                            await yielder.ReturnAsync(enumerator.Current, cancellationToken);
                        }
                    }
                }

                await yielder.BreakAsync(cancellationToken);
            });
        }

        public static IAsyncEnumerable<TSource> Where<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return Create<TSource>(async (yielder, cancellationToken) =>
            {
                var index = -1;
                using (var enumerator = source.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        index++;

                        if (predicate(enumerator.Current, index))
                        {
                            await yielder.ReturnAsync(enumerator.Current, cancellationToken);
                        }
                    }
                }

                await yielder.BreakAsync(cancellationToken);
            });

            //return new WhereAsyncIterator<TSource>(source, predicate);
        }

        private class WhereAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            private readonly IAsyncEnumerable<TSource> source;

            private readonly Func<TSource, int, bool> predicate;

            private int currentIndex = -1;

            private TSource current;

            private IAsyncEnumerator<TSource> sourceEnumerator;

            public WhereAsyncIterator(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override TSource Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new WhereAsyncIterator<TSource>(this.source, this.predicate);
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

                while (await this.sourceEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.currentIndex++;
                    this.current = this.sourceEnumerator.Current;

                    if (this.predicate(this.current, this.currentIndex++))
                    {
                        return true;
                    }
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

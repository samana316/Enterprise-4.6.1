using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, IEnumerable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, int, IEnumerable<TResult>> overload =
                (item, index) => selector(item);

            return new SelectManyAsyncIterator<TSource, TResult, TResult>(
                source,
                overload,
                (item, collection) => collection);
        }

        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SelectManyAsyncIterator<TSource, TResult, TResult>(
                source,
                selector,
                (item, collection) => collection);
        }

        public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new SelectManyAsyncIterator<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector);
        }

        public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            Func<TSource, int, IEnumerable<TCollection>> overload =
                (item, index) => collectionSelector(item);

            return new SelectManyAsyncIterator<TSource, TCollection, TResult>(
                source, overload, resultSelector);
        }

        private class SelectManyAsyncIterator<TSource, TCollection, TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable<TSource> source;

            private readonly Func<TSource, int, IEnumerable<TCollection>> collectionSelector;

            private readonly Func<TSource, TCollection, TResult> resultSelector;

            private IAsyncEnumerator<TSource> sourceIterator;

            private IEnumerator<TCollection> collectionIterator;

            private int currentIndex = -1;

            private TResult current;

            public SelectManyAsyncIterator(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
                Func<TSource, TCollection, TResult> resultSelector)
            {
                this.source = source;
                this.collectionSelector = collectionSelector;
                this.resultSelector = resultSelector;
            }

            public override TResult Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new SelectManyAsyncIterator<TSource, TCollection, TResult>(
                    this.source, this.collectionSelector, this.resultSelector);
            }

            public override void Reset()
            {
                this.Dispose();
                this.sourceIterator = null;
                this.collectionIterator = null;

                base.Reset();
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (this.sourceIterator != null)
                    {
                        this.sourceIterator.Dispose();
                    }

                    if (this.collectionIterator != null)
                    {
                        this.collectionIterator.Dispose();
                    }
                }

                base.Dispose(disposing);
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.sourceIterator == null)
                {
                    this.sourceIterator = this.source.GetAsyncEnumerator();

                    if (await this.sourceIterator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        this.currentIndex++;
                        this.collectionIterator = collectionSelector(
                            this.sourceIterator.Current, this.currentIndex).GetEnumerator();
                    }
                }

                if (collectionIterator.MoveNext())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.current = resultSelector(
                        this.sourceIterator.Current, collectionIterator.Current);

                    return true;
                }

                if (await this.sourceIterator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.currentIndex++;
                    this.collectionIterator.Dispose();
                    this.collectionIterator = collectionSelector(
                        this.sourceIterator.Current, this.currentIndex).GetEnumerator();

                    return await this.DoMoveNextAsync(cancellationToken);
                }

                return false;
            }
        }
    }
}

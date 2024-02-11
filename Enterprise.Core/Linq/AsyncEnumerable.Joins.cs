using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IAsyncEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IAsyncEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(outer, "outer");
            Check.NotNull(inner, "inner");
            Check.NotNull(outerKeySelector, "outerKeySelector");
            Check.NotNull(innerKeySelector, "innerKeySelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new GroupJoinAsyncIterator<TOuter, TInner, TKey, TResult>(
                    outer,
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    comparer);
        }

        public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IAsyncEnumerable<TOuter> outer, 
            IEnumerable<TInner> inner, 
            Func<TOuter, TKey> outerKeySelector, 
            Func<TInner, TKey> innerKeySelector, 
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IAsyncEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector, 
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(outer, "outer");
            Check.NotNull(inner, "inner");
            Check.NotNull(outerKeySelector, "outerKeySelector");
            Check.NotNull(innerKeySelector, "innerKeySelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new JoinAsyncIterator<TOuter, TInner, TKey, TResult>(
                    outer,
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    comparer);
        }

        private class GroupJoinAsyncIterator<TOuter, TInner, TKey, TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable<TOuter> outer;
            private readonly IEnumerable<TInner> inner;
            private readonly Func<TOuter, TKey> outerKeySelector;
            private readonly Func<TInner, TKey> innerKeySelector;
            private readonly Func<TOuter, IEnumerable<TInner>, TResult> resultSelector;
            private readonly IEqualityComparer<TKey> comparer;

            private IAsyncEnumerator<TOuter> enumerator;
            private AsyncLookup<TKey, TInner> lookup;
            private TResult result;

            public GroupJoinAsyncIterator(
                IAsyncEnumerable<TOuter> outer,
                IEnumerable<TInner> inner, 
                Func<TOuter, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector, 
                Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
                IEqualityComparer<TKey> comparer)
            {
                this.outer = outer;
                this.inner = inner;
                this.outerKeySelector = outerKeySelector;
                this.innerKeySelector = innerKeySelector;
                this.resultSelector = resultSelector;
                this.comparer = comparer;
            }

            public override TResult Current
            {
                get { return this.result; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new GroupJoinAsyncIterator<TOuter, TInner, TKey, TResult>(
                    outer,
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    comparer);
            }

            public override void Reset()
            {
                this.Dispose(true);
                base.Reset();
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (enumerator != null)
                    {
                        enumerator.Dispose();
                    }

                    enumerator = null;
                    lookup = null;
                }

                base.Dispose(disposing);
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (lookup == null)
                {
                    lookup = AsyncLookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
                }

                if (enumerator == null)
                {
                    enumerator = outer.GetAsyncEnumerator();
                }

                if (await enumerator.MoveNextAsync(cancellationToken))
                {
                    result = resultSelector(
                        enumerator.Current, lookup[outerKeySelector(enumerator.Current)]);

                    return true;
                }

                return false;
            }
        }

        private class JoinAsyncIterator<TOuter, TInner, TKey, TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable<TOuter> outer;
            private readonly IEnumerable<TInner> inner;
            private readonly Func<TOuter, TKey> outerKeySelector;
            private readonly Func<TInner, TKey> innerKeySelector;
            private readonly Func<TOuter, TInner, TResult> resultSelector;
            private readonly IEqualityComparer<TKey> comparer;

            private AsyncLookup<TKey, TInner> lookup;
            private IAsyncEnumerator<TOuter> outerIterator;
            private IAsyncEnumerator<TInner> innerIterator;
            private TResult result;

            public JoinAsyncIterator(
                IAsyncEnumerable<TOuter> outer,
                IEnumerable<TInner> inner,
                Func<TOuter, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, TInner, TResult> resultSelector,
                IEqualityComparer<TKey> comparer)
            {
                this.outer = outer;
                this.inner = inner;
                this.outerKeySelector = outerKeySelector;
                this.innerKeySelector = innerKeySelector;
                this.resultSelector = resultSelector;
                this.comparer = comparer;
            }

            public override TResult Current
            {
                get { return this.result; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new JoinAsyncIterator<TOuter, TInner, TKey, TResult>(
                    outer,
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    comparer);
            }

            public override void Reset()
            {
                this.Dispose(true);
                base.Reset();
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (this.innerIterator != null)
                    {
                        this.innerIterator.Dispose();
                    }

                    if (this.outerIterator != null)
                    {
                        this.outerIterator.Dispose();
                    }
                }

                base.Dispose(disposing);
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.lookup == null)
                {
                    this.lookup = AsyncLookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);

                    if (await this.lookup.GetAsyncEnumerator().MoveNextAsync(cancellationToken))
                    {
                        this.outerIterator = this.outer.GetAsyncEnumerator();

                        if (await this.outerIterator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                        {
                            var key = this.outerKeySelector(this.outerIterator.Current);
                            var grouping = this.lookup.GetGrouping(key, false);

                            if (grouping != null)
                            {
                                this.innerIterator = grouping.GetAsyncEnumerator();
                            }
                        }
                    }
                }

                if (this.outerIterator == null || this.innerIterator == null)
                {
                    return false;
                }

                if (await this.innerIterator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.result = this.resultSelector(
                        this.outerIterator.Current, this.innerIterator.Current);

                    return true;
                }

                if (await this.outerIterator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var key = this.outerKeySelector(this.outerIterator.Current);
                    var grouping = this.lookup.GetGrouping(key, false);

                    if (grouping != null)
                    {
                        this.innerIterator.Dispose();
                        this.innerIterator = grouping.GetAsyncEnumerator();

                        return await this.DoMoveNextAsync(cancellationToken);
                    }
                }

                return false;
            }
        }
    }
}

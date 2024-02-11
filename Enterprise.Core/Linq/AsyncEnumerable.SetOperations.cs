using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(
           this IAsyncEnumerable<TSource> source)
        {
            return source.DefaultIfEmpty(default(TSource));
        }

        public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IAsyncEnumerable<TSource> source, 
            TSource defaultValue)
        {
            Check.NotNull(source, "source");

            return new DefaultIfEmptyAsyncIterator<TSource>(source, defaultValue);
        }

        public static IAsyncEnumerable<TSource> Distinct<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.Distinct(null);
        }

        public static IAsyncEnumerable<TSource> Distinct<TSource>(
            this IAsyncEnumerable<TSource> source, 
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(source, "source");

            return new DisctinctAsyncIterator<TSource>(source, comparer);
        }

        public static IAsyncEnumerable<TSource> Except<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return first.Except(second, null);
        }

        public static IAsyncEnumerable<TSource> Except<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second, 
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new ExceptAsyncIterator<TSource>(first, second, comparer);
        }

        public static IAsyncEnumerable<TSource> Intersect<TSource>(
            this IAsyncEnumerable<TSource> first,
            IEnumerable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return first.Intersect(second, null);
        }

        public static IAsyncEnumerable<TSource> Intersect<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second, 
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new IntersectAsyncIterator<TSource>(first, second, comparer);
        }

        public static IAsyncEnumerable<TSource> Union<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return first.Union(second, null);
        }

        public static IAsyncEnumerable<TSource> Union<TSource>(
            this IAsyncEnumerable<TSource> first, 
            IEnumerable<TSource> second, 
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new UnionAsyncIterator<TSource>(first, second, comparer);
        }

        private abstract class SetOperationAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            protected readonly IAsyncEnumerable<TSource> first;
            protected readonly IAsyncEnumerable<TSource> second;
            protected readonly IEqualityComparer<TSource> comparer;

            protected Set<TSource> set;
            protected IAsyncEnumerator<TSource> firstEnumerator;
            protected IAsyncEnumerator<TSource> secondEnumerator;
            protected TSource current;

            protected SetOperationAsyncIterator(
                IAsyncEnumerable<TSource> first,
                IEnumerable<TSource> second,
                IEqualityComparer<TSource> comparer)
            {
                this.first = first;
                this.comparer = comparer;

                if (second != null)
                {
                    this.second = second.AsAsyncEnumerable();
                }
            }

            public override TSource Current
            {
                get { return this.current; }
            }

            public override void Reset()
            {
                this.Dispose(true);
                this.set = null;
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

            protected override sealed Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (this.set == null)
                {
                    this.set = new Set<TSource>(this.comparer);
                }

                if (this.firstEnumerator == null)
                {
                    this.firstEnumerator = this.first.GetAsyncEnumerator();
                }

                if (this.secondEnumerator == null && this.second != null)
                {
                    this.secondEnumerator = this.second.GetAsyncEnumerator();
                }

                return this.InternalMoveNextAsync(cancellationToken);
            }

            protected abstract Task<bool> InternalMoveNextAsync(CancellationToken cancellationToken);
        }

        private class DefaultIfEmptyAsyncIterator<TSource> : SetOperationAsyncIterator<TSource>
        {
            private readonly TSource defaultValue;

            private bool iterateDefault;

            public DefaultIfEmptyAsyncIterator(
                IAsyncEnumerable<TSource> source,
                TSource defaultValue)
                : base(source, null, null)
            {
                this.defaultValue = defaultValue;
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new DefaultIfEmptyAsyncIterator<TSource>(this.first, this.defaultValue);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.firstEnumerator.Current;
                    this.iterateDefault = true;
                    return true;
                }

                if (!iterateDefault)
                {
                    this.current = this.defaultValue;
                    this.iterateDefault = true;
                    return true;
                }

                return false;
            }
        }

        private class DisctinctAsyncIterator<TSource> : SetOperationAsyncIterator<TSource>
        {
            public DisctinctAsyncIterator(
                IAsyncEnumerable<TSource> source,
                IEqualityComparer<TSource> comparer)
                : base(source, null, comparer)
            {
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new DisctinctAsyncIterator<TSource>(this.first, this.comparer);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                while (await this.firstEnumerator.MoveNextAsync(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.current = this.firstEnumerator.Current;
                    
                    if (set.Add(this.current))
                    {
                        return true;
                    } 
                }

                return false;
            }
        }

        private class ExceptAsyncIterator<TSource> : SetOperationAsyncIterator<TSource>
        {
            private bool initialzed;

            public ExceptAsyncIterator(
                IAsyncEnumerable<TSource> first,
                IEnumerable<TSource> second,
                IEqualityComparer<TSource> comparer)
                : base(first, second, comparer)
            {
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new ExceptAsyncIterator<TSource>(this.first, this.second, this.comparer);
            }

            public override void Reset()
            {
                this.initialzed = false;
                base.Reset();
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (!this.initialzed)
                {
                    while (await this.secondEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        this.set.Add(this.secondEnumerator.Current);
                    }

                    this.initialzed = true;
                }

                while (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.firstEnumerator.Current;

                    if (this.set.Add(this.current))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private class IntersectAsyncIterator<TSource> : SetOperationAsyncIterator<TSource>
        {
            private bool initialzed;

            public IntersectAsyncIterator(
                IAsyncEnumerable<TSource> first,
                IEnumerable<TSource> second,
                IEqualityComparer<TSource> comparer)
                : base(first, second, comparer)
            {
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new IntersectAsyncIterator<TSource>(this.first, this.second, this.comparer);
            }

            public override void Reset()
            {
                this.initialzed = false;
                base.Reset();
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (!this.initialzed)
                {
                    while (await this.secondEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        this.set.Add(this.secondEnumerator.Current);
                    }

                    this.initialzed = true;
                }

                while (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.firstEnumerator.Current;

                    if (this.set.Remove(this.current))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private class UnionAsyncIterator<TSource> : SetOperationAsyncIterator<TSource>
        {
            public UnionAsyncIterator(
                IAsyncEnumerable<TSource> first,
                IEnumerable<TSource> second,
                IEqualityComparer<TSource> comparer)
                : base(first, second, comparer)
            {
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new UnionAsyncIterator<TSource>(this.first, this.second, this.comparer);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                while (await this.firstEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.firstEnumerator.Current;

                    if (this.set.Add(this.current))
                    {
                        return true;
                    }
                }

                while (await this.secondEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = this.secondEnumerator.Current;

                    if (this.set.Add(this.current))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

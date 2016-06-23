using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> Skip<TSource>(
            this IAsyncEnumerable<TSource> source, 
            int count)
        {
            Check.NotNull(source, "source");

            return new SkipAsyncIterator<TSource>(source, count);
        }

        public static IAsyncEnumerable<TSource> SkipWhile<TSource>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            Func<TSource, int, bool> overload = (item, index) => predicate(item);

            return new SkipWhileAsyncIterator<TSource>(source, overload);
        }

        public static IAsyncEnumerable<TSource> SkipWhile<TSource>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new SkipWhileAsyncIterator<TSource>(source, predicate);
        }

        public static IAsyncEnumerable<TSource> Take<TSource>(
            this IAsyncEnumerable<TSource> source,
            int count)
        {
            Check.NotNull(source, "source");

            return new TakeAsyncIterator<TSource>(source, count);
        }

        public static IAsyncEnumerable<TSource> TakeWhile<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            Func<TSource, int, bool> overload = (item, index) => predicate(item);

            return new TakeWhileAsyncIterator<TSource>(source, overload);
        }

        public static IAsyncEnumerable<TSource> TakeWhile<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new TakeWhileAsyncIterator<TSource>(source, predicate);
        }

        private abstract class PagingOperationAsyncIterator<TSource> : AsyncIterator<TSource>
        {
            protected readonly IAsyncEnumerable<TSource> source;

            protected IAsyncEnumerator<TSource> enumerator;

            protected int index = -1;

            protected int currentCount = -1;

            protected TSource current;

            protected PagingOperationAsyncIterator(
                IAsyncEnumerable<TSource> source)
            {
                this.source = source;
            }

            public override TSource Current
            {
                get { return this.current; }
            }

            public override void Reset()
            {
                this.currentCount = -1;
                this.index = -1;
                this.Dispose(true);
                this.enumerator = null;
                base.Reset();
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (this.enumerator != null)
                    {
                        this.enumerator.Dispose();
                    }
                }
                base.Dispose(disposing);
            }

            protected override sealed Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (this.enumerator == null)
                {
                    this.enumerator = this.source.GetAsyncEnumerator();
                }

                return this.InternalMoveNextAsync(cancellationToken);
            }

            protected abstract Task<bool> InternalMoveNextAsync(CancellationToken cancellationToken);
        }

        private class SkipAsyncIterator<TSource> : PagingOperationAsyncIterator<TSource>
        {
            private readonly int count;

            public SkipAsyncIterator(
                IAsyncEnumerable<TSource> source,
                int count)
                : base(source)
            {
                this.count = count;
                this.currentCount = this.count;
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new SkipAsyncIterator<TSource>(this.source, this.count);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                while (this.currentCount > 0 &&
                    await this.enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.currentCount--;
                }

                if (this.currentCount <= 0)
                {
                    if (await this.enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        this.current = this.enumerator.Current;
                        return true;
                    }
                }

                return false;
            }
        }

        private class SkipWhileAsyncIterator<TSource> : PagingOperationAsyncIterator<TSource>
        {
            private readonly Func<TSource, int, bool> predicate;

            public SkipWhileAsyncIterator(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, bool> predicate)
                : base(source)
            {
                this.predicate = predicate;
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new SkipWhileAsyncIterator<TSource>(this.source, this.predicate);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                var flag = false;

                checked
                {
                    while (await this.enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        index++;

                        this.current = this.enumerator.Current;
                        if (!flag && !this.predicate(this.current, index))
                        {
                            flag = true;
                        }

                        if (flag)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private class TakeAsyncIterator<TSource> : PagingOperationAsyncIterator<TSource>
        {
            private readonly int count;

            public TakeAsyncIterator(
                IAsyncEnumerable<TSource> source,
                int count)
                : base(source)
            {
                this.count = count;
                this.currentCount = this.count;
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new TakeAsyncIterator<TSource>(this.source, this.count);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.currentCount > 0)
                {
                    while (await this.enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        this.current = this.enumerator.Current;

                        if (--this.currentCount <= -1)
                        {
                            return false;
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        private class TakeWhileAsyncIterator<TSource> : PagingOperationAsyncIterator<TSource>
        {
            private readonly Func<TSource, int, bool> predicate;

            public TakeWhileAsyncIterator(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, bool> predicate)
                : base(source)
            {
                this.predicate = predicate;
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new TakeWhileAsyncIterator<TSource>(this.source, this.predicate);
            }

            protected override async Task<bool> InternalMoveNextAsync(
                CancellationToken cancellationToken)
            {
                checked
                {
                    while (await this.enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        index++;

                        this.current = this.enumerator.Current;
                        if (!this.predicate(this.current, index))
                        {
                            break;
                        }

                        return true;
                    }
                }

                return false;
            }
        }
    }
}

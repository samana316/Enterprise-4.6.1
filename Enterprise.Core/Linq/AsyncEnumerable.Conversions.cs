using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            return source;
        }

        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            this IEnumerable<TSource> source)
        {
            Check.NotNull(source, "source");

            var asyncEnumerable = source as IAsyncEnumerable<TSource>;
            if (asyncEnumerable != null)
            {
                return asyncEnumerable;
            }

            var asyncEnumerable2 = source as IAsyncEnumerable;
            if (asyncEnumerable2 != null)
            {
                return asyncEnumerable2.Cast<TSource>();
            }

            return new AsyncEnumerableAdapter<TSource>(source);
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            return source;
        }

        public static IAsyncEnumerable<TResult> OfType<TResult>(
            this IAsyncEnumerable source)
        {
            var enumerable = source as IAsyncEnumerable<TResult>;

            if (!ReferenceEquals(enumerable, null))
            {
                return enumerable;
            }

            Check.NotNull(source, "source");

            return new OfTypeAsyncIterator<TResult>(source);
        }

        public static IAsyncLookup<TKey, TSource> ToAsyncLookup<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return AsyncLookup<TKey, TSource>.Create(
                source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static IAsyncLookup<TKey, TSource> ToAsyncLookup<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IEqualityComparer<TKey> comparer)
        {
            return AsyncLookup<TKey, TSource>.Create(
                source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static IAsyncLookup<TKey, TElement> ToAsyncLookup<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector)
        {
            return AsyncLookup<TKey, TElement>.Create(
                source, keySelector, elementSelector, null);
        }

        public static IAsyncLookup<TKey, TElement> ToAsyncLookup<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            IEqualityComparer<TKey> comparer)
        {
            return AsyncLookup<TKey, TElement>.Create(
                source, keySelector, elementSelector, comparer);
        }

        private class AsyncEnumerableAdapter<TSource> : AsyncIterator<TSource>
        {
            private readonly IEnumerable<TSource> source;

            private IEnumerator<TSource> sourceEnumerator;

            private TSource current;

            public AsyncEnumerableAdapter(
                IEnumerable<TSource> source)
            {
                this.source = source;
            }

            public override TSource Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TSource> Clone()
            {
                return new AsyncEnumerableAdapter<TSource>(this.source);
            }

            protected override Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                Func<bool> func = this.InternalMoveNext;

                return func.InvokeAsync(cancellationToken);
            }

            private bool InternalMoveNext()
            {
                if (this.sourceEnumerator == null)
                {
                    this.sourceEnumerator = this.source.GetEnumerator();
                }

                if (this.sourceEnumerator.MoveNext())
                {
                    this.current = this.sourceEnumerator.Current;

                    return true;
                }

                return false;
            }
        }

        private class CastAsyncIterator<TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable source;

            private IAsyncEnumerator sourceEnumerator;

            private TResult current;

            public CastAsyncIterator(
                IAsyncEnumerable source)
            {
                this.source = source;
            }

            public override TResult Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new CastAsyncIterator<TResult>(this.source);
            }

            protected override async Task<bool> DoMoveNextAsync(
                CancellationToken cancellationToken)
            {
                if (this.sourceEnumerator == null)
                {
                    this.sourceEnumerator = this.source.GetAsyncEnumerator();
                }

                if (await this.sourceEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    this.current = (TResult)this.sourceEnumerator.Current;
                    return true;
                }

                return false;
            }
        }

        private class OfTypeAsyncIterator<TResult> : AsyncIterator<TResult>
        {
            private readonly IAsyncEnumerable source;

            private IAsyncEnumerator sourceEnumerator;

            private TResult current;

            public OfTypeAsyncIterator(
                IAsyncEnumerable source)
            {
                this.source = source;
            }

            public override TResult Current
            {
                get { return this.current; }
            }

            public override AsyncIterator<TResult> Clone()
            {
                return new CastAsyncIterator<TResult>(this.source);
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
                    if (this.sourceEnumerator.Current is TResult)
                    {
                        this.current = (TResult)this.sourceEnumerator.Current;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

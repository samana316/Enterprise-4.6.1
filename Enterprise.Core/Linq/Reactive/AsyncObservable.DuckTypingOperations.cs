using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Runtime.CompilerServices;
using Enterprise.Core.Common.Runtime.ExceptionServices;
using Enterprise.Core.Resources;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncEnumerator<TSource> GetAsyncEnumerator<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.ToAsyncEnumerable().GetAsyncEnumerator();
        }

        public static IEnumerator<TSource> GetEnumerator<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.GetAsyncEnumerator();
        }

        public static IAwaiter<TSource> GetAwaiter<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            var flyweights = AsyncObservableAwaiterFlyweights<TSource>.Instance;

            var flyweight = flyweights.SingleOrDefault(item => ReferenceEquals(item.Key, source));

            if (!ReferenceEquals(flyweight.Value, null))
            {
                return flyweight.Value;
            }

            var awaiter = new AsyncObservableAwaiter<TSource>(source, CancellationToken.None);
            flyweights.Add(source, awaiter);

            return awaiter;
        }

        private sealed class AsyncObservableAwaiterFlyweights<TSource>
        {
            private static readonly IDictionary<IAsyncObservable<TSource>, IAwaiter<TSource>> flyweights
                = new Dictionary<IAsyncObservable<TSource>, IAwaiter<TSource>>(new ReferenceComparer<IAsyncObservable<TSource>>());

            private AsyncObservableAwaiterFlyweights()
            {
            }

            public static IDictionary<IAsyncObservable<TSource>, IAwaiter<TSource>> Instance
            {
                get { return flyweights; }
            }
        }

        private sealed class AsyncObservableAwaiter<TSource> : IAwaiter<TSource>
        {
            private readonly Task<TSource> task;

            public AsyncObservableAwaiter(
                IAsyncObservable<TSource> source,
                CancellationToken cancellationToken)
            {
                this.task = source.ToAsyncEnumerable().LastAsync(cancellationToken);
            }

            public bool IsCompleted
            {
                get
                {
                    return this.task.Status == TaskStatus.Canceled ||
                        this.task.Status == TaskStatus.Faulted ||
                        this.task.Status == TaskStatus.RanToCompletion;
                }
            }

            public TSource GetResult()
            {
                return this.task.Result;
            }

            void IAwaiter.GetResult()
            {
                this.GetResult();
            }

            public void OnCompleted(
                Action continuation)
            {
                continuation();
            }
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(
                T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(
                T obj)
            {
                return new object().GetHashCode();
            }
        }
    }
}

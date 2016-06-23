using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Repeat<TResult>(
            this IAsyncObservable<TResult> source)
        {
            return Create<TResult>(async (observer, cancellationToken) => 
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await source.ForEachAsync(observer.OnNextAsync, cancellationToken);
                }
            });
        }

        public static IAsyncObservable<TResult> Repeat<TResult>(
            this IAsyncObservable<TResult> source,
            int count)
        {
            if (count < 0)
            {
                throw Error.ArgumentOutOfRange("count");
            }

            return AsyncEnumerable.Repeat(source, count).Concat();
        }

        public static IAsyncObservable<TResult> Repeat<TResult>(
           TResult element)
        {
            return new RepeatAsyncObservable<TResult>(element, null);
        }

        public static IAsyncObservable<TResult> Repeat<TResult>(
           TResult element,
           int count)
        {
            if (count < 0)
            {
                throw Error.ArgumentOutOfRange("count");
            }

            return new RepeatAsyncObservable<TResult>(element, count);
        }

        private sealed class RepeatAsyncObservable<TResult> : AsyncObservableBase<TResult>
        {
            private readonly TResult element;

            private readonly int? count;

            public RepeatAsyncObservable(
                TResult element, 
                int? count)
            {
                this.element = element;
                this.count = count;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                if (this.count.HasValue)
                {
                    for (var i = 0; i < count.Value; i++)
                    {
                        await observer.OnNextAsync(element, cancellationToken);
                    }
                }
                else
                {
                    while (true)
                    {
                        await observer.OnNextAsync(element, cancellationToken);
                    }
                }

                return null;
            }
        }
    }
}

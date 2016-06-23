using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Linq.Reactive.Impl;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> AsAsyncObservable<TSource>(
            this IAsyncObservable<TSource> source)
        {
            return source;
        }

        public static IAsyncObservable<TSource> AsAsyncObservable<TSource>(
            this IObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            var observable = source as IAsyncObservable<TSource>;
            if (observable != null)
            {
                return observable;
            }

            return new AsyncObservableAdapter<TSource>(source);
        }

        public static IAsyncObservable<TSource[]> ToArray<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new ToArrayAsyncObservable<TSource>(source);
        }

        public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            var asyncEnumerable = source as IAsyncEnumerable<TSource>;
            if (asyncEnumerable != null)
            {
                return asyncEnumerable;
            }

            return new AsyncObservableIterator<TSource>(source);
        }

        public static IAsyncObservable<TSource> ToAsyncObservable<TSource>(
           this IEnumerable<TSource> source)
        {
            Check.NotNull(source, "source");

            var observable = source as IAsyncObservable<TSource>;
            if (observable != null)
            {
                return observable;
            }

            return new AsyncEnumerableObservableAdapter<TSource>(source.AsAsyncEnumerable());
        }

        public static IAsyncObservable<IList<TSource>> ToList<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new ToListAsyncObservable<TSource>(source);
        }

        private sealed class AsyncEnumerableObservableAdapter<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncEnumerable<TSource> source;

            public AsyncEnumerableObservableAdapter(
                IAsyncEnumerable<TSource> source)
            {
                this.source = source;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var enumerator = this.source.GetAsyncEnumerator();

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await observer.OnNextAsync(enumerator.Current, cancellationToken);
                    }
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                finally
                {
                    observer.OnCompleted();
                }

                return enumerator;
            }
        }

        private sealed class AsyncObservableAdapter<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IObservable<TSource> source;

            public AsyncObservableAdapter(
                IObservable<TSource> source)
            {
                this.source = source;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                Func<IDisposable> func = () => this.source.Subscribe(observer);

                return func.InvokeAsync(cancellationToken);
            }
        }

        private sealed class AsyncObservableIterator<TSource> : 
            AsyncObservableImplBase2<TSource>,  IAsyncEnumerable<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            public AsyncObservableIterator(
                IAsyncObservable<TSource> source)
            {
                this.source = source;
            }

            public IAsyncEnumerator<TSource> GetAsyncEnumerator()
            {
                return this.InternalGetAsyncEnumerator();
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                return this.GetAsyncEnumerator();
            }

            IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
            {
                return this.GetAsyncEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetAsyncEnumerator();
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                return this.source.SubscribeSafeAsync(observer, cancellationToken);
            }
        }

        private sealed class ToArrayAsyncObservable<TSource> : AsyncObservableBase<TSource[]>
        {
            private readonly IAsyncObservable<TSource> source;

            public ToArrayAsyncObservable(
                IAsyncObservable<TSource> source)
            {
                this.source = source;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource[]> observer,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var list = new List<TSource>();
                await source.ForEachAsync(list.Add, cancellationToken);

                var result = new ReturnAsyncObservable<TSource[]>(list.ToArray());

                return await result.SubscribeAsync(observer, cancellationToken);
            }
        }

        private sealed class ToListAsyncObservable<TSource> : AsyncObservableBase<IList<TSource>>
        {
            private readonly IAsyncObservable<TSource> source;

            public ToListAsyncObservable(
                IAsyncObservable<TSource> source)
            {
                this.source = source;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<IList<TSource>> observer, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var list = new List<TSource>();
                await source.ForEachAsync(list.Add, cancellationToken);

                var result = new ReturnAsyncObservable<IList<TSource>>(list);

                return await result.SubscribeAsync(observer, cancellationToken);
            }
        }
    }
}

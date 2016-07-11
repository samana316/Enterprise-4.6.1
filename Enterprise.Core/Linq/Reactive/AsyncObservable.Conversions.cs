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

        public static IAsyncObservable<TResult> Cast<TResult>(
            this IAsyncObservable<object> source)
        {
            return source.Cast<object, TResult>();
        }

        public static IAsyncObservable<TResult> Cast<TSource, TResult>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            var observable = source as IAsyncObservable<TResult>;
            if (observable != null)
            {
                return observable;
            }

            return new CastAsyncObservable<TSource, TResult>(source);
        }

        public static IAsyncObservable<TResult> OfType<TResult>(
            this IAsyncObservable<object> source)
        {
            return source.OfType<object, TResult>();
        }

        public static IAsyncObservable<TResult> OfType<TSource, TResult>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            var observable = source as IAsyncObservable<TResult>;
            if (observable != null)
            {
                return observable;
            }

            return new OfTypeAsyncObservable<TSource, TResult>(source);
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

        public static IAsyncObservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return new ToDictionaryAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, null);
        }

        public static IAsyncObservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(comparer, "comparer");

            return new ToDictionaryAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, comparer);
        }

        public static IAsyncObservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return new ToDictionaryAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, null);
        }

        public static IAsyncObservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");
            Check.NotNull(comparer, "comparer");

            return new ToDictionaryAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, comparer);
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

        private sealed class CastAsyncObservable<TSource, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TSource> source;

            public CastAsyncObservable(
                IAsyncObservable<TSource> source)
            {
                this.source = source;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                var castImpl = new CastAsyncObserver(observer);

                return this.source.SubscribeSafeAsync(castImpl, cancellationToken);
            }

            private sealed class CastAsyncObserver : AsyncSink<TSource>
            {
                private readonly IAsyncObserver<TResult> observer;

                public CastAsyncObserver(
                    IAsyncObserver<TResult> observer)
                    : base(observer.AsPartial())
                {
                    this.observer = observer;
                }

                protected override Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    var result = (TResult)(object)value;

                    return this.observer.OnNextAsync(result, cancellationToken);
                }
            }
        }

        private sealed class OfTypeAsyncObservable<TSource, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TSource> source;

            public OfTypeAsyncObservable(
                IAsyncObservable<TSource> source)
            {
                this.source = source;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer,
                CancellationToken cancellationToken)
            {
                var ofTypeImpl = new OfTypeAsyncObserver(observer);

                return this.source.SubscribeSafeAsync(ofTypeImpl, cancellationToken);
            }

            private sealed class OfTypeAsyncObserver : AsyncSink<TSource>
            {
                private readonly IAsyncObserver<TResult> observer;

                public OfTypeAsyncObserver(
                    IAsyncObserver<TResult> observer)
                    : base(observer.AsPartial())
                {
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (value is TResult)
                    {
                        var result = (TResult)(object)value;

                        await this.observer.OnNextAsync(result, cancellationToken);
                    }
                }
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

        private sealed class ToDictionaryAsyncObservable<TSource, TKey, TElement>
            : AsyncObservableBase<IDictionary<TKey, TElement>>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, TKey> keySelector;

            private readonly Func<TSource, TElement> elementSelector;

            private readonly IEqualityComparer<TKey> comparer;

            public ToDictionaryAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, TKey> keySelector, 
                Func<TSource, TElement> elementSelector, 
                IEqualityComparer<TKey> comparer)
            {
                this.source = source;
                this.keySelector = keySelector;
                this.elementSelector = elementSelector;
                this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<IDictionary<TKey, TElement>> observer, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dictionary = new Dictionary<TKey, TElement>(this.comparer);

                await this.source.ForEachAsync(
                    item => dictionary.Add(this.keySelector(item), this.elementSelector(item)),
                    cancellationToken);

                var result = Return(dictionary);

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

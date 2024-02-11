using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> SelectMany<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, IObservable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, int, IObservable<TResult>> overload =
                (item, index) => selector(item);

            return new SelectManyAsyncObservable<TSource, TResult, TResult>(
                source,
                overload,
                (item, collection) => collection);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, IObservable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SelectManyAsyncObservable<TSource, TResult, TResult>(
                source,
                selector,
                (item, collection) => collection);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, IObservable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new SelectManyAsyncObservable<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, IObservable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            Func<TSource, int, IObservable<TCollection>> overload =
                (item, index) => collectionSelector(item);

            return new SelectManyAsyncObservable<TSource, TCollection, TResult>(
                source, overload, resultSelector);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TResult>(
           this IAsyncObservable<TSource> source,
           Func<TSource, IEnumerable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            Func<TSource, int, IEnumerable<TResult>> overload =
                (item, index) => selector(item);

            return new SelectManyAsyncObservable<TSource, TResult, TResult>(
                source,
                overload,
                (item, collection) => collection);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new SelectManyAsyncObservable<TSource, TResult, TResult>(
                source,
                selector,
                (item, collection) => collection);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new SelectManyAsyncObservable<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector);
        }

        public static IAsyncObservable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IAsyncObservable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(collectionSelector, "collectionSelector");
            Check.NotNull(resultSelector, "resultSelector");

            Func<TSource, int, IEnumerable<TCollection>> overload =
                (item, index) => collectionSelector(item);

            return new SelectManyAsyncObservable<TSource, TCollection, TResult>(
                source, overload, resultSelector);
        }

        private sealed class SelectManyAsyncObservable<TSource, TCollection, TResult> : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, int, IObservable<TCollection>> observableSelector;

            private readonly Func<TSource, int, IEnumerable<TCollection>> enumerableSelector;

            private readonly Func<TSource, TCollection, TResult> resultSelector;

            private IAsyncObserver<TResult> observer;

            private TSource current;

            public SelectManyAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, int, IObservable<TCollection>> collectionSelector, 
                Func<TSource, TCollection, TResult> resultSelector)
            {
                this.source = source;
                this.observableSelector = collectionSelector;
                this.resultSelector = resultSelector;
            }

            public SelectManyAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
                Func<TSource, TCollection, TResult> resultSelector)
            {
                this.source = source;
                this.enumerableSelector = collectionSelector;
                this.resultSelector = resultSelector;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                this.observer = observer;
                await source.ForEachAsync(this.RunOuterAsync, cancellationToken);

                return null;
            }

            private Task RunOuterAsync(
                TSource item,
                int index,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IAsyncObservable<TCollection> collection = null;

                if (this.observableSelector != null)
                {
                    collection = this.observableSelector(item, index).AsAsyncObservable();
                }
                else if (this.enumerableSelector != null)
                {
                    collection = this.enumerableSelector(item, index).ToAsyncObservable();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                this.current = item;
                return collection.ForEachAsync(this.RunInnerAsync, cancellationToken);
            }

            private Task RunInnerAsync(
                TCollection collection,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = this.resultSelector(this.current, collection);

                return this.observer.OnNextAsync(result, cancellationToken);
            }
        }
    }
}

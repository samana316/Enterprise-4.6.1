using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq.Reactive.Subjects;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector)
        {
            return new GroupByAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, null, null);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new GroupByAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, null, comparer);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return new GroupByAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, null, null);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            IEqualityComparer<TKey> comparer)
        {
            return new GroupByAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, null, comparer);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            int capacity)
        {
            return new GroupByAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, capacity, null);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            int capacity, 
            IEqualityComparer<TKey> comparer)
        {
            return new GroupByAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, capacity, comparer);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            int capacity)
        {
            return new GroupByAsyncObservable<TSource, TKey, TSource>(
                source, keySelector, IdentityFunction, capacity, null);
        }

        public static IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            int capacity, 
            IEqualityComparer<TKey> comparer)
        {
            return new GroupByAsyncObservable<TSource, TKey, TElement>(
                source, keySelector, elementSelector, capacity, comparer);
        }

        private sealed class GroupByAsyncObservable<TSource, TKey, TElement> :
            AsyncObservableBase<IAsyncGroupedObservable<TKey, TElement>>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, TKey> keySelector;

            private readonly Func<TSource, TElement> elementSelector;

            private readonly int? capacity;

            private readonly IEqualityComparer<TKey> comparer;

            public GroupByAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, TKey> keySelector, 
                Func<TSource, TElement> elementSelector, 
                int? capacity, 
                IEqualityComparer<TKey> comparer)
            {
                Check.NotNull(source, "source");
                Check.NotNull(keySelector, "keySelector");
                Check.NotNull(elementSelector, "elementSelector");

                this.source = source;
                this.keySelector = keySelector;
                this.elementSelector = elementSelector;
                this.capacity = capacity;
                this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<IAsyncGroupedObservable<TKey, TElement>> observer,
                CancellationToken cancellationToken)
            {
                var groupByImpl = new GroupByAsyncObserver(this, observer);

                return this.source.SubscribeRawAsync(groupByImpl, cancellationToken);
            }

            private sealed class GroupByAsyncObserver : AsyncSink<TSource>
            {
                private readonly GroupByAsyncObservable<TSource, TKey, TElement> parent;

                private readonly IDictionary<TKey, IAsyncSubject<TElement>> map;

                private readonly IAsyncObserver<IAsyncGroupedObservable<TKey, TElement>> observer;

                private IAsyncSubject<TElement> currentSubject;

                public GroupByAsyncObserver(
                    GroupByAsyncObservable<TSource, TKey, TElement> parent,
                    IAsyncObserver<IAsyncGroupedObservable<TKey, TElement>> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;

                    if (this.parent.capacity.HasValue)
                    {
                        this.map = new Dictionary<TKey, IAsyncSubject<TElement>>(
                            this.parent.capacity.Value, this.parent.comparer);

                        return;
                    }

                    this.map = new Dictionary<TKey, IAsyncSubject<TElement>>(this.parent.comparer);
                }

                protected override void OnCompletedCore()
                {
                    if (this.currentSubject != null)
                    {
                        this.currentSubject.OnCompleted();
                    }

                    foreach (var current in this.map.Values)
                    {
                        current.OnCompleted();
                    }

                    base.OnCompletedCore();
                }

                protected override void OnErrorCore(
                    Exception error)
                {
                    if (this.currentSubject != null)
                    {
                        this.currentSubject.OnError(error);
                    }

                    foreach (var current in this.map.Values)
                    {
                        current.OnError(error);
                    }
                    
                    base.OnErrorCore(error);
                }

                protected override async Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    var key = default(TKey);

                    try
                    {
                        key = this.parent.keySelector(value);
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                        return;
                    }

                    var flag = false;
                    IAsyncSubject<TElement> subject = null;
                    try
                    {
                        if (ReferenceEquals(key, null))
                        {
                            if (ReferenceEquals(this.currentSubject, null))
                            {
                                this.currentSubject = new AsyncSubject<TElement>();
                                flag = true;
                            }

                            subject = this.currentSubject;
                        }
                        else if (!this.map.TryGetValue(key, out subject))
                        {
                            subject = new AsyncSubject<TElement>();
                            this.map.Add(key, subject);
                            flag = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                        return;
                    }

                    if (flag)
                    {
                        var group = new AsyncGroupedObservable<TKey, TElement>(key, subject);
                        await this.observer.OnNextAsync(group, cancellationToken);
                    }

                    var element = default(TElement);
                    try
                    {
                        element = this.parent.elementSelector(value);
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                        return;
                    }

                    await subject.OnNextAsync(element, cancellationToken);
                }
            }
        }
    }
}

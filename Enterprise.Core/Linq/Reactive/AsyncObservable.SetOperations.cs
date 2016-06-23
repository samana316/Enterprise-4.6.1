using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> DefaultIfEmpty<TSource>(
          this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new DefaultIfEmptyAsyncObservable<TSource>(source, default(TSource));
        }

        public static IAsyncObservable<TSource> DefaultIfEmpty<TSource>(
            this IAsyncObservable<TSource> source,
            TSource defaultValue)
        {
            Check.NotNull(source, "source");

            return new DefaultIfEmptyAsyncObservable<TSource>(source, defaultValue);
        }

        public static IAsyncObservable<TSource> Distinct<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new DistinctAsyncObservable<TSource, TSource>(source, IdentityFunction, null);
        }

        public static IAsyncObservable<TSource> Distinct<TSource>(
            this IAsyncObservable<TSource> source,
            IEqualityComparer<TSource> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(comparer, "comparer");

            return new DistinctAsyncObservable<TSource, TSource>(source, IdentityFunction, comparer);
        }

        public static IAsyncObservable<TSource> Distinct<TSource, TKey>(
            this IAsyncObservable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return new DistinctAsyncObservable<TSource, TKey>(source, keySelector, null);
        }

        public static IAsyncObservable<TSource> Distinct<TSource, TKey>(
            this IAsyncObservable<TSource> source, 
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(comparer, "comparer");

            return new DistinctAsyncObservable<TSource, TKey>(source, keySelector, comparer);
        }

        private sealed class DefaultIfEmptyAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly TSource defaultValue;

            public DefaultIfEmptyAsyncObservable(
                IAsyncObservable<TSource> source, 
                TSource defaultValue)
            {
                this.source = source;
                this.defaultValue = defaultValue;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var defaultIfEmptyImpl = new DefaultIfEmptyAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(defaultIfEmptyImpl, cancellationToken);
            }

            private sealed class DefaultIfEmptyAsyncObserver : AsyncSink<TSource>
            {
                private readonly DefaultIfEmptyAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private bool found;

                public DefaultIfEmptyAsyncObserver(
                    DefaultIfEmptyAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    this.found = true;
                    return this.observer.OnNextAsync(value, cancellationToken);
                }

                protected override void OnCompletedCore()
                {
                    if (!this.found)
                    {
                        this.observer.OnNextAsync(this.parent.defaultValue).Wait();
                    }

                    base.OnCompletedCore();
                }
            }
        }

        private sealed class DistinctAsyncObservable<TSource, TKey> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, TKey> keySelector;

            private readonly IEqualityComparer<TKey> comparer;

            public DistinctAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, TKey> keySelector, 
                IEqualityComparer<TKey> comparer)
            {
                this.source = source;
                this.keySelector = keySelector;
                this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var distinctImpl = new DistinctAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(distinctImpl, cancellationToken);
            }

            private sealed class DistinctAsyncObserver : AsyncSink<TSource>
            {
                private readonly DistinctAsyncObservable<TSource, TKey> parent;

                private readonly IAsyncObserver<TSource> observer;

                private readonly ISet<TKey> set;

                public DistinctAsyncObserver(
                    DistinctAsyncObservable<TSource, TKey> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                    this.set = new HashSet<TKey>(this.parent.comparer);
                }

                protected override async Task OnNextCoreAsync(
                    TSource value, 
                    CancellationToken cancellationToken)
                {
                    var item = this.parent.keySelector(value);
                    var flag = this.set.Add(item);

                    if (flag)
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                }
            }
        }
    }
}

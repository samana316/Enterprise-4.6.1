using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> ElementAt<TSource>(
            this IAsyncObservable<TSource> source,
            int index)
        {
            Check.NotNull(source, "source");

            var list = source as IReadOnlyList<TSource>;
            if (list != null)
            {
                var element = list[index];
                return Return(element);
            }

            if (index < 0)
            {
                throw Error.ArgumentOutOfRange("index");
            }

            return new ElementAtAsyncObservable<TSource>(source, index, true);
        }

        public static IAsyncObservable<TSource> ElementAtOrDefault<TSource>(
            this IAsyncObservable<TSource> source,
            int index)
        {
            Check.NotNull(source, "source");

            return new ElementAtAsyncObservable<TSource>(source, index, false);
        }

        public static IAsyncObservable<TSource> First<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new FirstAsyncObservable<TSource>(source, null, true);
        }

        public static IAsyncObservable<TSource> First<TSource>(
           this IAsyncObservable<TSource> source,
           Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new FirstAsyncObservable<TSource>(source, predicate, true);
        }

        public static IAsyncObservable<TSource> FirstOrDefault<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new FirstAsyncObservable<TSource>(source, null, false);
        }

        public static IAsyncObservable<TSource> FirstOrDefault<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new FirstAsyncObservable<TSource>(source, predicate, false);
        }

        public static IAsyncObservable<TSource> Last<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new LastAsyncObservable<TSource>(source, null, true);
        }

        public static IAsyncObservable<TSource> Last<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new LastAsyncObservable<TSource>(source, predicate, true);
        }

        public static IAsyncObservable<TSource> LastOrDefault<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new LastAsyncObservable<TSource>(source, null, false);
        }

        public static IAsyncObservable<TSource> LastOrDefault<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new LastAsyncObservable<TSource>(source, predicate, false);
        }

        public static IAsyncObservable<TSource> Single<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new SingleAsyncObservable<TSource>(source, null, true);
        }

        public static IAsyncObservable<TSource> Single<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new SingleAsyncObservable<TSource>(source, predicate, true);
        }

        public static IAsyncObservable<TSource> SingleOrDefault<TSource>(
            this IAsyncObservable<TSource> source)
        {
            Check.NotNull(source, "source");

            return new SingleAsyncObservable<TSource>(source, null, false);
        }

        public static IAsyncObservable<TSource> SingleOrDefault<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new SingleAsyncObservable<TSource>(source, predicate, false);
        }

        private sealed class ElementAtAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly int index;

            private readonly bool throwOnEmpty;

            public ElementAtAsyncObservable(
                IAsyncObservable<TSource> source,
                int index,
                bool throwOnEmpty)
            {
                this.source = source;
                this.index = index;
                this.throwOnEmpty = throwOnEmpty;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var elementAtImpl = new ElementAtAsyncObserver(this, observer);

                var subscription = await this.source.SubscribeRawAsync(elementAtImpl, cancellationToken);

                await elementAtImpl.OnCompletedAsync(cancellationToken);

                return subscription;
            }

            private sealed class ElementAtAsyncObserver : AsyncSink<TSource>
            {
                private readonly ElementAtAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int index;

                private bool found;

                public ElementAtAsyncObserver(
                    ElementAtAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                    this.index = parent.index;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.index == 0)
                    {
                        this.found = true;
                        await this.observer.OnNextAsync(value, cancellationToken);
                        this.OnCompleted();
                    }

                    this.index--;
                }

                internal async Task OnCompletedAsync(
                    CancellationToken cancellationToken)
                {
                    if (!this.found)
                    {
                        if (this.parent.throwOnEmpty)
                        {
                            throw Error.ArgumentOutOfRange("index");
                        }
                        else
                        {
                            await this.observer.OnNextAsync(default(TSource), cancellationToken);
                        }
                    }

                    this.OnCompletedCore();
                }
            }
        }

        private sealed class FirstAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            private readonly bool throwOnEmpty;

            public FirstAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate,
                bool throwOnEmpty)
            {
                this.source = source;
                this.predicate = predicate;
                this.throwOnEmpty = throwOnEmpty;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var firstImpl = new FirstAsyncObserver(this, observer);

                var subscription = await this.source.SubscribeRawAsync(firstImpl, cancellationToken);

                await firstImpl.OnCompletedAsync(cancellationToken);

                return subscription;
            }

            private sealed class FirstAsyncObserver : AsyncSink<TSource>
            {
                private readonly FirstAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private bool found;

                public FirstAsyncObserver(
                    FirstAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    this.found = this.parent.predicate == null || this.parent.predicate(value);

                    if (this.found)
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                        this.OnCompleted();
                    }
                }

                internal async Task OnCompletedAsync(
                    CancellationToken cancellationToken)
                {
                    if (!this.found)
                    {
                        if (this.parent.throwOnEmpty)
                        {
                            throw this.parent.predicate == null ?
                                Error.EmptySequence() : Error.NoMatch();
                        }
                        else
                        {
                            await this.observer.OnNextAsync(default(TSource), cancellationToken);
                        }
                    }

                    this.OnCompletedCore();
                }
            }
        }

        private sealed class LastAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            private readonly bool throwOnEmpty;

            public LastAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate,
                bool throwOnEmpty)
            {
                this.source = source;
                this.predicate = predicate;
                this.throwOnEmpty = throwOnEmpty;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var lastImpl = new LastAsyncObserver(this, observer);

                var subscription = await this.source.SubscribeRawAsync(lastImpl, cancellationToken);

                await lastImpl.OnCompletedAsync(cancellationToken);

                return subscription;
            }

            private sealed class LastAsyncObserver : AsyncSink<TSource>
            {
                private readonly LastAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private TSource value = default(TSource);

                private bool found;

                public LastAsyncObserver(
                    LastAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.parent.predicate == null || this.parent.predicate(value))
                    {
                        this.found = true;
                        this.value = value;
                    }

                    await Task.Yield();
                }

                internal async Task OnCompletedAsync(
                    CancellationToken cancellationToken)
                {
                    if (!this.found && this.parent.throwOnEmpty)
                    {
                        throw this.parent.predicate == null ?
                            Error.EmptySequence() : Error.NoMatch();
                    }
                    else
                    {
                        await this.observer.OnNextAsync(this.value, cancellationToken);
                    }

                    this.OnCompletedCore();
                }
            }
        }

        private sealed class SingleAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            private readonly bool throwOnEmpty;

            public SingleAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate,
                bool throwOnEmpty)
            {
                this.source = source;
                this.predicate = predicate;
                this.throwOnEmpty = throwOnEmpty;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var singleImpl = new SingleAsyncObserver(this, observer);

                var subscription = await this.source.SubscribeRawAsync(singleImpl, cancellationToken);

                await singleImpl.OnCompletedAsync(cancellationToken);

                return subscription;
            }

            private sealed class SingleAsyncObserver : AsyncSink<TSource>
            {
                private readonly SingleAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private TSource value = default(TSource);

                private bool found;

                public SingleAsyncObserver(
                    SingleAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.found)
                    {
                        throw this.parent.predicate == null ?
                            Error.MoreThanOneElement() : Error.MoreThanOneMatch();
                    }

                    this.found = this.parent.predicate == null || this.parent.predicate(value);
                    this.value = value;

                    await Task.Yield();
                }

                internal async Task OnCompletedAsync(
                    CancellationToken cancellationToken)
                {
                    if (this.found)
                    {
                        await this.observer.OnNextAsync(this.value, cancellationToken);
                    }
                    else
                    {
                        if (this.parent.throwOnEmpty)
                        {
                            throw this.parent.predicate == null ?
                                Error.EmptySequence() : Error.NoMatch();
                        }
                        else
                        {
                            await this.observer.OnNextAsync(default(TSource), cancellationToken);
                        }
                    }

                    this.OnCompletedCore();
                }
            }
        }
    }
}

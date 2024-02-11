using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Skip<TSource>(
            this IAsyncObservable<TSource> source,
            int count)
        {
            Check.NotNull(source, "source");

            return new SkipAsyncObservable<TSource>(source, count);
        }

        public static IAsyncObservable<TSource> SkipWhile<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new SkipWhileAsyncObservable<TSource>(source, predicate);
        }

        public static IAsyncObservable<TSource> SkipWhile<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new SkipWhileAsyncObservable<TSource>(source, predicate);
        }

        public static IAsyncObservable<TSource> Take<TSource>(
            this IAsyncObservable<TSource> source,
            int count)
        {
            Check.NotNull(source, "source");

            return new TakeAsyncObservable<TSource>(source, count);
        }

        public static IAsyncObservable<TSource> TakeWhile<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new TakeWhileAsyncObservable<TSource>(source, predicate);
        }

        public static IAsyncObservable<TSource> TakeWhile<TSource>(
            this IAsyncObservable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return new TakeWhileAsyncObservable<TSource>(source, predicate);
        }

        private class SkipAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly int count;

            public SkipAsyncObservable(
                IAsyncObservable<TSource> source,
                int count)
            {
                this.source = source;
                this.count = count;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var SkipImpl = new SkipAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(SkipImpl, cancellationToken);
            }

            private sealed class SkipAsyncObserver : AsyncSink<TSource>
            {
                private readonly SkipAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int remaining;

                public SkipAsyncObserver(
                    SkipAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.remaining = this.parent.count;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.remaining <= 0)
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                        return;
                    }

                    this.remaining--;
                }
            }
        }

        private class SkipWhileAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            private readonly Func<TSource, int, bool> predicateI;

            public SkipWhileAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public SkipWhileAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, int, bool> predicate)
            {
                this.source = source;
                this.predicateI = predicate;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer,
                CancellationToken cancellationToken)
            {
                var SkipWhileImpl = new SkipWhileAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(SkipWhileImpl, cancellationToken);
            }

            private sealed class SkipWhileAsyncObserver : AsyncSink<TSource>
            {
                private readonly SkipWhileAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int index;

                private bool running;

                public SkipWhileAsyncObserver(
                    SkipWhileAsyncObservable<TSource> parent,
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
                    cancellationToken.ThrowIfCancellationRequested();

                    if (this.parent.predicate != null)
                    {
                        return this.SkipWhileAsync(value, cancellationToken);
                    }

                    return this.SkipWhileImplAsync(value, cancellationToken);
                }

                private async Task SkipWhileAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (!this.running)
                    {
                        this.running = !this.parent.predicate(value);
                    }

                    if (this.running)
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                }

                private async Task SkipWhileImplAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (!this.running)
                    {
                        this.running = !this.parent.predicateI(value, this.index++);
                    }
                     
                    if (this.running)
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                }
            }
        }

        private class TakeAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly int count;

            public TakeAsyncObservable(
                IAsyncObservable<TSource> source, 
                int count)
            {
                this.source = source;
                this.count = count;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var takeImpl = new TakeAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(takeImpl, cancellationToken);
            }

            private sealed class TakeAsyncObserver : AsyncSink<TSource>
            {
                private readonly TakeAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int remaining;

                public TakeAsyncObserver(
                    TakeAsyncObservable<TSource> parent,
                    IAsyncObserver<TSource> observer)
                    : base(observer.AsPartial())
                {
                    this.parent = parent;
                    this.remaining = this.parent.count;
                    this.observer = observer;
                }

                protected override async Task OnNextCoreAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.remaining > 0)
                    {
                        this.remaining--;
                        await this.observer.OnNextAsync(value, cancellationToken);

                        if (this.remaining <= 0)
                        {
                            this.observer.OnCompleted();
                        }
                    }
                }
            }
        }

        private class TakeWhileAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<TSource, bool> predicate;

            private readonly Func<TSource, int, bool> predicateI;

            public TakeWhileAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public TakeWhileAsyncObservable(
                IAsyncObservable<TSource> source,
                Func<TSource, int, bool> predicate)
            {
                this.source = source;
                this.predicateI = predicate;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var takeWhileImpl = new TakeWhileAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(takeWhileImpl, cancellationToken);
            }

            private sealed class TakeWhileAsyncObserver : AsyncSink<TSource>
            {
                private readonly TakeWhileAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private int index;

                public TakeWhileAsyncObserver(
                    TakeWhileAsyncObservable<TSource> parent,
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
                    cancellationToken.ThrowIfCancellationRequested();

                    if (this.parent.predicate != null)
                    {
                        return this.TakeWhileAsync(value, cancellationToken);
                    }

                    return this.TakeWhileImplAsync(value, cancellationToken);
                }

                private async Task TakeWhileAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.parent.predicate(value))
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                    else
                    {
                        this.observer.OnCompleted();
                    }
                }

                private async Task TakeWhileImplAsync(
                    TSource value,
                    CancellationToken cancellationToken)
                {
                    if (this.parent.predicateI(value, this.index++))
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                    else
                    {
                        this.observer.OnCompleted();
                    }
                }
            }
        }
    }
}

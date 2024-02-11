using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Catch<TSource, TException>(
            this IAsyncObservable<TSource> source, 
            Func<TException, IAsyncObservable<TSource>> handler) 
            where TException : Exception
        {
            Check.NotNull(source, "source");
            Check.NotNull(handler, "handler");

            return new CatchAsyncObservable<TSource, TException>(source, handler);
        }

        public static IAsyncObservable<TSource> Catch<TSource>(
            this IAsyncObservable<TSource> first,
            IAsyncObservable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new CatchAsyncObservable<TSource>(new[] { first, second });
        }

        public static IAsyncObservable<TSource> Catch<TSource>(
            this IEnumerable<IAsyncObservable<TSource>> sources)
        {
            Check.NotNull(sources, "sources");

            return new CatchAsyncObservable<TSource>(sources);
        }

        public static IAsyncObservable<TSource> Catch<TSource>(
            params IAsyncObservable<TSource>[] sources)
        {
            Check.NotNull(sources, "sources");

            return sources.AsEnumerable().Catch();
        }

        private sealed class CatchAsyncObservable<TSource, TException> : AsyncObservableBase<TSource>
            where TException : Exception
        {
            private readonly Func<TException, IAsyncObservable<TSource>> handler;

            private readonly IAsyncObservable<TSource> source;

            public CatchAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<TException, IAsyncObservable<TSource>> handler)
            {
                this.source = source;
                this.handler = handler;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                IDisposable subscription = null;

                try
                {
                    subscription = await this.source.SubscribeRawAsync(observer, cancellationToken);
                }
                catch (TException exception)
                {
                    var observable = this.handler(exception);

                    subscription = await observable.SubscribeRawAsync(observer, cancellationToken);
                }

                return subscription;
            }
        }

        private sealed class CatchAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncEnumerable<IAsyncObservable<TSource>> sources;

            public CatchAsyncObservable(
                IEnumerable<IAsyncObservable<TSource>> sources)
            {
                this.sources = sources.AsAsyncEnumerable();
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var catchImpl = new CatchAsyncObservableImpl(this, observer);

                return catchImpl.RunAsync(cancellationToken);
            }

            private sealed class CatchAsyncObservableImpl : IProducer
            {
                private readonly CatchAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private bool isCompleted;

                public CatchAsyncObservableImpl(
                    CatchAsyncObservable<TSource> parent, 
                    IAsyncObserver<TSource> observer)
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                public async Task<IDisposable> RunAsync(
                    CancellationToken cancellationToken)
                {
                    var enumerator = this.parent.sources.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        if (this.isCompleted)
                        {
                            break;
                        }

                        var catchImpl = new CatchAsyncObserver(this);
                        await enumerator.Current.SubscribeAsync(catchImpl, cancellationToken);
                    }

                    return enumerator;
                }

                private sealed class CatchAsyncObserver : AsyncSink<TSource>
                {
                    private readonly CatchAsyncObservableImpl parent;

                    private bool hasError;

                    public CatchAsyncObserver(
                        CatchAsyncObservableImpl parent)
                        : base(parent.observer.AsPartial())
                    {
                        this.parent = parent;
                    }

                    protected override Task OnNextCoreAsync(
                        TSource value, 
                        CancellationToken cancellationToken)
                    {
                        return this.parent.observer.OnNextAsync(value, cancellationToken);
                    }

                    protected override void OnErrorCore(
                        Exception error)
                    {
                        this.hasError = true;
                        base.OnErrorCore(error);
                    }

                    protected override void OnCompletedCore()
                    {
                        if (!this.hasError)
                        {
                            this.parent.isCompleted = true;
                        }

                        base.OnCompletedCore();
                    }
                }
            }
        }
    }
}

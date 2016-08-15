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
        public static IAsyncObservable<TSource> Amb<TSource>(
            this IAsyncObservable<TSource> first,
            IAsyncObservable<TSource> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new AmbAsyncObservable<TSource>(first, second);
        }

        public static IAsyncObservable<TSource> Amb<TSource>(
            this IEnumerable<IAsyncObservable<TSource>> sources)
        {
            Check.NotNull(sources, "sources");

            return sources.Aggregate(
                Never<TSource>(), (previous, current) => previous.Amb(current));
        }

        public static IAsyncObservable<TSource> Amb<TSource>(
            params IAsyncObservable<TSource>[] sources)
        {
            Check.NotNull(sources, "sources");

            return sources.AsEnumerable().Amb();
        }

        private sealed class AmbAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private enum AmbState
            {
                Neither = 0,
                Left,
                Right,
            }

            private readonly IAsyncObservable<TSource> first;

            private readonly IAsyncObservable<TSource> second;

            public AmbAsyncObservable(
                IAsyncObservable<TSource> first, 
                IAsyncObservable<TSource> second)
            {
                this.first = first;
                this.second = second;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var ambImpl = new AmbAsyncObservableImpl(this, observer);

                return ambImpl.RunAsync(cancellationToken);
            }

            private class AmbAsyncObservableImpl : IProducer
            {
                private readonly AmbAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                private AmbState choice;

                public AmbAsyncObservableImpl(
                    AmbAsyncObservable<TSource> parent, 
                    IAsyncObserver<TSource> observer)
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                public Task<IDisposable> RunAsync(
                    CancellationToken cancellationToken)
                {
                    var decision1 = new DecisionAsyncObserver(this, AmbState.Left);
                    var decision2 = new DecisionAsyncObserver(this, AmbState.Right);

                    var task1 = this.parent.first.SubscribeAsync(decision1, cancellationToken);
                    var task2 = this.parent.second.SubscribeAsync(decision2, cancellationToken);

                    return Task.WhenAny(task1, task2).Unwrap();
                }

                private class DecisionAsyncObserver : AsyncSink<TSource>
                {
                    private readonly AmbAsyncObservableImpl parent;

                    private readonly AmbState me;

                    public DecisionAsyncObserver(
                        AmbAsyncObservableImpl parent,
                        AmbState me)
                        : base(parent.observer.AsPartial())
                    {
                        this.parent = parent;
                        this.me = me;
                    }

                    protected override async Task OnNextCoreAsync(
                        TSource value,
                        CancellationToken cancellationToken)
                    {
                        if (this.parent.choice == AmbState.Neither)
                        {
                            this.parent.choice = this.me;
                        }

                        if (this.parent.choice == this.me)
                        {
                            await this.parent.observer.OnNextAsync(value, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}

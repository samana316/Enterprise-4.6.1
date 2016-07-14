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

                internal bool found;

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
    }
}

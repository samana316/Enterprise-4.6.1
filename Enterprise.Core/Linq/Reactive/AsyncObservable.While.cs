using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> While<TSource>(
            this IAsyncObservable<TSource> source,
            Func<bool> condition)
        {
            Check.NotNull(source, "source");
            Check.NotNull(condition, "condition");

            return new WhileAsyncObservable<TSource>(source, condition);
        }

        private sealed class WhileAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private readonly Func<bool> condition;

            public WhileAsyncObservable(
                IAsyncObservable<TSource> source, 
                Func<bool> condition)
            {
                this.source = source;
                this.condition = condition;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                var whileImpl = new WhileAsyncObserver(this, observer);

                return this.source.SubscribeSafeAsync(whileImpl, cancellationToken);
            }

            private sealed class WhileAsyncObserver : AsyncSink<TSource>
            {
                private readonly WhileAsyncObservable<TSource> parent;

                private readonly IAsyncObserver<TSource> observer;

                public WhileAsyncObserver(
                    WhileAsyncObservable<TSource> parent,
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
                    cancellationToken.ThrowIfCancellationRequested();

                    if (this.parent.condition())
                    {
                        await this.observer.OnNextAsync(value, cancellationToken);
                    }
                    else
                    {
                        this.OnCompleted();
                    }
                }
            }
        }
    }
}

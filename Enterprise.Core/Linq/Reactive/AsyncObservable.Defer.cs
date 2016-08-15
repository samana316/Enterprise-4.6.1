using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq.Reactive.Subjects;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> Defer<TResult>(
            this IAsyncObservable<TResult> source)
        {
            Check.NotNull(source, "source");

            return new DeferAsyncObservable<TResult>(source);
        }

        private sealed class DeferAsyncObservable<TResult> : AsyncRealSubject<TResult>
        {
            private readonly IAsyncObservable<TResult> source;

            private bool isRunning;

            public DeferAsyncObservable(
                IAsyncObservable<TResult> source)
            {
                this.source = source;
            }

            public override async Task<IDisposable> SubscribeAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                var subscription = await base.SubscribeAsync(observer, cancellationToken);

                if (!this.isRunning)
                {
                    var task = this.source.SubscribeRawAsync(this.observers, cancellationToken);

                    this.isRunning = true;
                }

                return subscription;
            }
        }
    }
}

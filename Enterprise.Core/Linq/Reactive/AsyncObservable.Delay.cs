using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> Delay<TSource>(
            this IAsyncObservable<TSource> source,
            TimeSpan dueTime)
        {
            Check.NotNull(source, "source");

            return new DelayAsyncObservable<TSource>(source, dueTime);
        }

        private sealed class DelayAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly TimeSpan dueTime;

            private readonly IAsyncObservable<TSource> source;

            public DelayAsyncObservable(
                IAsyncObservable<TSource> source, 
                TimeSpan dueTime)
            {
                this.source = source;
                this.dueTime = dueTime;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                await Task.Delay(this.dueTime, cancellationToken);
                
                return await this.source.SubscribeRawAsync(observer, cancellationToken);
            }
        }
    }
}

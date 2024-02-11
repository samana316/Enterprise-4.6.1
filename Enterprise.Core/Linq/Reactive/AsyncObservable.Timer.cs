using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<long> Interval(
            TimeSpan period)
        {
            return new TimerAsyncObservable(period);
        }

        public static IAsyncObservable<long> Timer(
            TimeSpan dueTime)
        {
            return new TimerAsyncObservable(dueTime);
        }

        public static IAsyncObservable<long> Timer(
            TimeSpan dueTime,
            TimeSpan period)
        {
            return new TimerAsyncObservable(dueTime, period);
        }

        private sealed class TimerAsyncObservable : AsyncObservableBase<long>
        {
            private readonly object sink = new object();

            private readonly TimeSpan dueTime;

            private readonly TimeSpan period;

            private bool condition;

            public TimerAsyncObservable(
                TimeSpan dueTime)
            {
                this.dueTime = dueTime;
            }

            public TimerAsyncObservable(
                TimeSpan dueTime, 
                TimeSpan period)
            {
                this.dueTime = dueTime;
                this.period = period;
                this.condition = true;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<long> observer, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var count = 1L;

                await Task.Delay(this.dueTime, cancellationToken);
                await observer.OnNextAsync(count, cancellationToken);
                
                while (this.condition)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    count++;

                    await Task.Delay(this.period, cancellationToken);
                    await observer.OnNextAsync(count, cancellationToken);
                }

                return null;
            }
        }
    }
}

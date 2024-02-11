using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Linq.Reactive;
using Timer = System.Timers.Timer;

namespace TestWindowsForms.Controllers
{
    internal sealed class ClassicTimer : DisposableBase, IAsyncObservable<long>
    {
        private readonly TimeSpan dueTime;
        private readonly TimeSpan period;

        private Timer timer;

        public ClassicTimer(
            TimeSpan dueTime, 
            TimeSpan period)
        {
            this.dueTime = dueTime;
            this.period = period;
        }

        public IDisposable Subscribe(
            IObserver<long> observer)
        {
            if (this.timer == null)
            {
                this.timer = new Timer();
            }

            this.timer.Interval = this.dueTime.TotalMilliseconds + 1;
            this.timer.Elapsed += async (sender, e) =>
            {
                await observer.OnNextAsync(0);
                this.timer.Interval = this.period.TotalMilliseconds;
            };
            this.timer.Start();

            return new Subscription(this.timer);
        }

        public Task<IDisposable> SubscribeAsync(
            IAsyncObserver<long> observer, 
            CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Subscribe(observer), cancellationToken);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }

            base.Dispose(disposing);
        }

        private class Subscription : DisposableBase
        {
            private readonly Timer timer;

            public Subscription(
                Timer timer)
            {
                this.timer = timer;
            }

            protected override void Dispose(
                bool disposing)
            {
                this.timer.Stop();
                this.timer.Dispose();

                base.Dispose(disposing);
            }
        }
    }
}

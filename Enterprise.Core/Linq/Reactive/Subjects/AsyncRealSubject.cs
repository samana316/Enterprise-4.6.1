using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive.Subjects
{
    internal abstract class AsyncRealSubject<T> : DisposableBase, IAsyncSubject<T>
    {
        private readonly object sink = new object();

        internal readonly CompositeAsyncObserver<T> observers = new CompositeAsyncObserver<T>();

        internal readonly ICollection<IDisposable> subscriptions = new List<IDisposable>();

        public void OnCompleted()
        {
            this.observers.OnCompleted();
        }

        public void OnError(
            Exception error)
        {
            this.observers.OnError(error);
        }

        public void OnNext(
            T value)
        {
            this.observers.OnNext(value);
        }

        public Task OnNextAsync(
            T value, 
            CancellationToken cancellationToken)
        {
            return this.observers.OnNextAsync(value, cancellationToken);
        }

        public IDisposable Subscribe(
            IObserver<T> observer)
        {
            Check.NotNull(observer, "observer");

            return this.InternalSubscribe(observer.AsAsyncObserver());
        }

        public virtual async Task<IDisposable> SubscribeAsync(
            IAsyncObserver<T> observer, 
            CancellationToken cancellationToken)
        {
            Check.NotNull(observer, "observer");

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Yield();
            return this.InternalSubscribe(observer);
        }

        private IDisposable InternalSubscribe(
            IAsyncObserver<T> observer)
        {
            lock (sink)
            {
                if (!this.observers.Contains(observer))
                {
                    this.observers.Add(observer);
                }

                var subscription = new Subscription(this, observer);

                this.subscriptions.Add(subscription);

                return subscription;
            }
        }

        protected override void Dispose(
            bool disposing)
        {
            lock (sink)
            {
                if (disposing)
                {
                    foreach (var subscription in this.subscriptions)
                    {
                        if (ReferenceEquals(subscription, null))
                        {
                            continue;
                        }

                        subscription.Dispose();
                    }

                    this.observers.Clear();
                }

                base.Dispose(disposing);
            }
        }

        [Serializable]
        private class Subscription : DisposableBase
        {
            private readonly object sink = new object();

            private readonly AsyncRealSubject<T> observable;

            private IAsyncObserver<T> observer;

            public Subscription(
                AsyncRealSubject<T> observable,
                IAsyncObserver<T> observer)
            {
                this.observable = observable;
                this.observer = observer;
            }

            protected override sealed void Dispose(
                bool disposing)
            {
                lock (sink)
                {
                    if (disposing)
                    {
                        if (this.observer != null &&
                            this.observable.observers.Contains(this.observer))
                        {
                            this.observable.observers.Remove(this.observer);
                        }
                    }

                    base.Dispose(disposing);
                }
            }
        }
    }
}

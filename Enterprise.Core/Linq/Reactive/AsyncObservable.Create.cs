using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<T> Create<T>(
            Func<IAsyncObserver<T>, Task> subscribeAsync)
        {
            Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> overload =
                async (observer, cancellationToken) => { await subscribeAsync(observer); return null; };

            return new AnonymousAsyncObservable<T>(overload);
        }

        public static IAsyncObservable<T> Create<T>(
            Func<IAsyncObserver<T>, CancellationToken, Task> subscribeAsync)
        {
            Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> overload =
                async (observer, cancellationToken) => { await subscribeAsync(observer, cancellationToken); return null; };

            return new AnonymousAsyncObservable<T>(overload);
        }

        public static IAsyncObservable<T> Create<T>(
            Func<IAsyncObserver<T>, Task<IDisposable>> subscribeAsync)
        {
            Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> overload = 
                (observer, cancellationToken) => subscribeAsync(observer);

            return new AnonymousAsyncObservable<T>(overload);
        }

        public static IAsyncObservable<T> Create<T>(
            Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> subscribeAsync)
        {
            return new AnonymousAsyncObservable<T>(subscribeAsync);
        }

        private sealed class AnonymousAsyncObservable<T> : AsyncObservableBase<T>
        {
            private readonly Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> subscribeAsync;

            public AnonymousAsyncObservable(
                Func<IAsyncObserver<T>, CancellationToken, Task<IDisposable>> subscribeAsync)
            {
                Check.NotNull(subscribeAsync, "subscribeAsync");

                this.subscribeAsync = subscribeAsync;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<T> observer, 
                CancellationToken cancellationToken)
            {
                return this.subscribeAsync(observer, cancellationToken);
            }
        }
    }
}

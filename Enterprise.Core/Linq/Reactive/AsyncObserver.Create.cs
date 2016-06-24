using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObserver
    {
        public static IAsyncObserver<T> Create<T>(
            Func<T, CancellationToken, Task> onNextAsync)
        {
            return new AnonymousAsyncObserver<T>(onNextAsync, NoOp, NoOp);
        }

        public static IAsyncObserver<T> Create<T>(
            Func<T, CancellationToken, Task> onNextAsync,
            Action<Exception> onError)
        {
            return new AnonymousAsyncObserver<T>(onNextAsync, onError, NoOp);
        }

        public static IAsyncObserver<T> Create<T>(
            Func<T, CancellationToken, Task> onNextAsync,
            Action onCompleted)
        {
            return new AnonymousAsyncObserver<T>(onNextAsync, NoOp, onCompleted);
        }

        public static IAsyncObserver<T> Create<T>(
            Func<T, CancellationToken, Task> onNextAsync,
            Action<Exception> onError,
            Action onCompleted)
        {
            return new AnonymousAsyncObserver<T>(onNextAsync, onError, onCompleted);
        }

        internal static IAsyncObserver<T> NoOp<T>()
        {
            return NoOpAsyncObserver<T>.Instance;
        }

        private static void NoOp()
        {
        }

        private static void NoOp<T>(
            T arg)
        {
        }

        private sealed class AnonymousAsyncObserver<T> : AsyncObserverBase<T>
        {
            private readonly Func<T, CancellationToken, Task> onNextAsync;

            private readonly Action<Exception> onError;

            private readonly Action onCompleted;

            public AnonymousAsyncObserver(
                Func<T, CancellationToken, Task> onNextAsync, 
                Action<Exception> onError, 
                Action onCompleted)
            {
                Check.NotNull(onNextAsync, "onNextAsync");
                Check.NotNull(onError, "onError");
                Check.NotNull(onCompleted, "onCompleted");

                this.onNextAsync = onNextAsync;
                this.onError = onError;
                this.onCompleted = onCompleted;
            }

            protected override void OnCompletedCore()
            {
                this.onCompleted();
            }

            protected override void OnErrorCore(
                Exception error)
            {
                this.onError(error);
            }

            protected override Task OnNextCoreAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                return this.onNextAsync(value, cancellationToken);
            }
        }

        private sealed class NoOpAsyncObserver<T> : AsyncObserverBase<T>
        {
            public static NoOpAsyncObserver<T> Instance = new NoOpAsyncObserver<T>();

            private NoOpAsyncObserver()
            {
            }

            protected override void OnCompletedCore()
            {
            }

            protected override void OnErrorCore(
                Exception error)
            {
            }

            protected override async Task OnNextCoreAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                await Task.Yield();
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObserver
    {
        public static IAsyncObserver<TSource> AsAsyncObserver<TSource>(
            this IAsyncObserver<TSource> observer)
        {
            return observer;
        }

        public static IAsyncObserver<TSource> AsAsyncObserver<TSource>(
            this IObserver<TSource> observer)
        {
            Check.NotNull(observer, "observer");

            return new AsyncObserverAdapter<TSource>(observer);
        }

        internal static IPartialObserver AsPartial<TSource>(
            this IObserver<TSource> observer)
        {
            Check.NotNull(observer, "observer");

            return new PartialObserverAdapter<TSource>(observer);
        }

        private sealed class AsyncObserverAdapter<T> : IAsyncObserver<T>
        {
            private readonly IObserver<T> observer;

            public AsyncObserverAdapter(
                IObserver<T> observer)
            {
                this.observer = observer;
            }

            public void OnCompleted()
            {
                this.observer.OnCompleted();
            }

            public void OnError(
                Exception error)
            {
                this.observer.OnError(error);
            }

            public void OnNext(
                T value)
            {
                this.observer.OnNext(value);
            }

            public Task OnNextAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                Func<object> func = () => { this.observer.OnNext(value); return null; };

                return func.InvokeAsync(cancellationToken);
            }
        }

        private class PartialObserverAdapter<T> : IPartialObserver
        {
            private IObserver<T> observer;

            public PartialObserverAdapter(
                IObserver<T> observer)
            {
                this.observer = observer;
            }

            public void OnCompleted()
            {
                this.observer.OnCompleted();
            }

            public void OnError(
                Exception error)
            {
                this.observer.OnError(error);
            }
        }
    }
}

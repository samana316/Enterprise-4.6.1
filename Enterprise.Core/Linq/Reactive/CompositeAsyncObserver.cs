using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading;

namespace Enterprise.Core.Linq.Reactive
{
    internal sealed class CompositeAsyncObserver<T> : AsyncObserverBase<T>, ICollection<IAsyncObserver<T>>
    {
        private readonly ICollection<IAsyncObserver<T>> observers = new List<IAsyncObserver<T>>();

        private readonly object sink = new object();

        public int Count
        {
            get
            {
                return this.observers.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.observers.IsReadOnly;
            }
        }

        public void Add(
            IAsyncObserver<T> item)
        {
            this.observers.Add(item);
        }

        public void Clear()
        {
            this.observers.Clear();
        }

        public bool Contains(
            IAsyncObserver<T> item)
        {
            return this.observers.Contains(item);
        }

        public void CopyTo(
            IAsyncObserver<T>[] array, 
            int arrayIndex)
        {
            this.observers.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IAsyncObserver<T>> GetEnumerator()
        {
            return this.observers.GetEnumerator();
        }

        public bool Remove(
            IAsyncObserver<T> item)
        {
            return this.observers.Remove(item);
        }

        protected override void Dispose(
            bool disposing)
        {
            lock (sink)
            {
                foreach (var disposable in this.observers.ToArray().OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }

        protected override void OnCompletedCore()
        {
            lock (sink)
            {
                foreach (var observer in this.observers.ToArray())
                {
                    if (observer != null)
                    {
                        observer.OnCompleted();
                    }
                }
            }
        }

        protected override void OnErrorCore(
            Exception error)
        {
            lock (sink)
            {
                foreach (var observer in this.observers)
                {
                    if (observer != null)
                    {
                        observer.OnError(error);
                    }
                }
            }
        }

        protected override async Task OnNextCoreAsync(
            T value, 
            CancellationToken cancellationToken)
        {
            using (await AsyncLock.LockAsync(sink))
            {
                foreach (var observer in this.observers)
                {
                    if (observer != null)
                    {
                        await observer.OnNextAsync(value, cancellationToken);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.observers.GetEnumerator();
        }
    }
}

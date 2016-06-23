using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;

namespace Enterprise.Tests.Linq.Reactive.Helpers
{
    internal class TestAsyncObserver<T> : IAsyncObserver<T>
    {
        private readonly List<T> items = new List<T>();

        public IAsyncEnumerable<T> Items
        {
            get { return this.items.AsAsyncEnumerable(); }
        }

        public virtual void OnCompleted()
        {
            Trace.WriteLine(DateTime.Now, "OnCompleted");
        }

        public virtual void OnError(
            Exception error)
        {
            Trace.WriteLine(error, "OnError");
        }

        public void OnNext(
            T value)
        {
            throw new InvalidOperationException("Synchronous version should not be called.");
        }

        public virtual Task OnNextAsync(
            T value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            this.items.Add(value);

            return Console.Out.WriteLineAsync(
                string.Format(
                    "OnNextAsync {0}: {1}",
                    value,
                    DateTime.Now));
        }

        public void Reset()
        {
            this.items.Clear();
        }
    }
}

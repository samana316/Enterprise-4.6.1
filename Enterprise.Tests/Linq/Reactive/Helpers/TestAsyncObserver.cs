﻿using System;
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
        private readonly Guid instanceId = Guid.NewGuid();

        private readonly ICollection<T> items = new List<T>();

        private readonly ICollection<Exception> errors = new List<Exception>();

        private readonly ICollection<StackTrace> stackTraces = new List<StackTrace>();

        public IAsyncEnumerable<T> Items
        {
            get { return this.items.AsAsyncEnumerable(); }
        }

        public IAsyncEnumerable<Exception> Errors
        {
            get { return this.errors.AsAsyncEnumerable(); }
        }

        public IAsyncEnumerable<StackTrace> StackTraces
        {
            get { return this.stackTraces.AsAsyncEnumerable(); }
        }

        public bool IsCompleted { get; private set; }

        public virtual void OnCompleted()
        {
            this.IsCompleted = true;

            this.stackTraces.Add(new StackTrace());

            Trace.WriteLine(DateTime.Now, "OnCompleted");
            Trace.WriteLine(this.instanceId, "InstanceId");
        }

        public virtual void OnError(
            Exception error)
        {
            this.errors.Add(error);
            Trace.WriteLine(error, "OnError");
        }

        public virtual void OnNext(
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
                    "OnNextAsync {0}: {1}, InstanceId: {2}",
                    value,
                    DateTime.Now,
                    this.instanceId));
        }

        public void Reset()
        {
            this.items.Clear();
            this.errors.Clear();
            this.stackTraces.Clear();
            this.IsCompleted = false;
        }
    }
}

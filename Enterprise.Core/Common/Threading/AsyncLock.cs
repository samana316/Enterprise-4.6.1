using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Common.Threading
{
    public static class AsyncLock
    {
        private static readonly IDictionary<object, WeakReference> ResetEventMap =
            new Dictionary<object, WeakReference>();

        public static IDisposable Lock(
            object resource)
        {
            var resetEvent = ResetEventFor(resource);

            resetEvent.WaitOne();
            resetEvent.Reset();

            return new ExitDisposable(resource);
        }

        public static Task<IDisposable> LockAsync(
            object resource)
        {
            return LockAsync(resource, CancellationToken.None);
        }

        public static Task<IDisposable> LockAsync(
            object resource,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run<IDisposable>(() =>
            {
                var resetEvent = ResetEventFor(resource);

                resetEvent.WaitOne();
                resetEvent.Reset();

                return new ExitDisposable(resource);
            });
        }

        private static ManualResetEvent ResetEventFor(
            object @lock)
        {
            if (!ResetEventMap.ContainsKey(@lock) ||
                !ResetEventMap[@lock].IsAlive)
            {
                ResetEventMap[@lock] =
                    new WeakReference(new ManualResetEvent(true));
            }

            return ResetEventMap[@lock].Target as ManualResetEvent;
        }

        private static void CleanUp()
        {
            ResetEventMap.Where(kv => !kv.Value.IsAlive)
                         .ToList()
                         .ForEach(kv => ResetEventMap.Remove(kv));
        }

        private class ExitDisposable : IDisposable
        {
            private readonly object _lock;

            public ExitDisposable(object @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                ResetEventFor(_lock).Set();
            }

            ~ExitDisposable()
            {
                CleanUp();
            }
        }
    }
}

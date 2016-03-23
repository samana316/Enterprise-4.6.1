using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Linq;
using Enterprise.Core.ObjectCreations;

namespace Enterprise.Core.IO
{
    internal abstract class AsyncIterator<TSource> :
        DisposableBase,
        IAsyncEnumerable<TSource>,
        IAsyncEnumerator<TSource>,
        IPrototype<AsyncIterator<TSource>>
    {
        private readonly int threadId = Thread.CurrentThread.ManagedThreadId;

        private int state;

        public virtual TSource Current { get; protected set; }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        public abstract AsyncIterator<TSource> Clone();

        public IAsyncEnumerator<TSource> GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        public virtual bool MoveNext()
        {
            return this.MoveNextAsync(CancellationToken.None).Result;
        }

        public abstract Task<bool> MoveNextAsync(CancellationToken cancellationToken);

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(
            bool disposing)
        {
            this.Current = default(TSource);
            this.state = -1;

            base.Dispose(disposing);
        }

        private AsyncIterator<TSource> InternalGetAsyncEnumerator()
        {
            if (this.threadId == Thread.CurrentThread.ManagedThreadId &&
                this.state == 0)
            {
                this.state = 1;
                return this;
            }

            var iterator = this.Clone();
            iterator.state = 1;

            return iterator;
        }
    }
}

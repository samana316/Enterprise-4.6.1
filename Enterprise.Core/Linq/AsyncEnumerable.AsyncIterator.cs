using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.ObjectCreations;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        private abstract class AsyncIterator<TSource> :
            DisposableBase,
            IAsyncEnumerable<TSource>,
            IAsyncEnumerator<TSource>,
            IPrototype<AsyncIterator<TSource>>
        {
            private readonly int threadId = Thread.CurrentThread.ManagedThreadId;

            private int state;

            public abstract TSource Current { get; }

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

            public bool MoveNext()
            {
                return this.MoveNextAsync(CancellationToken.None).Result;
            }

            public async Task<bool> MoveNextAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (this.state)
                {
                    case 1:
                        this.state = 2;
                        break;
                    case 2:
                        break;
                    default:
                        return false;
                }

                return await this.DoMoveNextAsync(cancellationToken);
            }

            public virtual void Reset()
            {
            }

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

            protected abstract Task<bool> DoMoveNextAsync(CancellationToken cancellationToken);

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
}

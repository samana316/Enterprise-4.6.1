using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    partial class AsyncLookup<TKey, TElement>
    {
        IAsyncEnumerable<TElement> IAsyncLookup<TKey, TElement>.this[TKey key]
        {
            get
            {
                AsyncGrouping grouping = this.GetGrouping(key, false);
                if (grouping != null)
                {
                    return grouping;
                }
                return AsyncEnumerable.Empty<TElement>();
            }
        }

        public IAsyncEnumerable<TResult> ApplyResultSelector<TResult>(
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return AsyncEnumerable.CreateBufferred<TResult>((yielder, cancellationToken) =>
                this.StateMachineIterator(resultSelector, yielder, cancellationToken));
        }

        public IAsyncEnumerator<IAsyncGrouping<TKey, TElement>> GetAsyncEnumerator()
        {
            return AsyncEnumerable.CreateBufferred<IAsyncGrouping<TKey, TElement>>(
                this.StateMachineIterator).GetAsyncEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        IEnumerator<IAsyncGrouping<TKey, TElement>> IEnumerable<IAsyncGrouping<TKey, TElement>>.GetEnumerator()
        {
            return this.GetAsyncEnumerator();
        }

        private async Task StateMachineIterator(
            IAsyncYielder<IAsyncGrouping<TKey, TElement>> yielder,
            CancellationToken cancellationToken)
        {
            var next = this.lastGrouping;
            if (next != null)
            {
                do
                {
                    next = next.next;

                    await yielder.ReturnAsync(next, cancellationToken);
                }
                while (next != this.lastGrouping);
            }

            await yielder.BreakAsync(cancellationToken);
        }

        private async Task StateMachineIterator<TResult>(
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IAsyncYielder<TResult> yielder,
            CancellationToken cancellationToken)
        {
            var next = this.lastGrouping;
            if (next != null)
            {
                do
                {
                    next = next.next;
                    if (next.count != next.elements.Length)
                    {
                        Array.Resize(ref next.elements, next.count);
                    }

                    var result = resultSelector(next.key, next.elements);
                    await yielder.ReturnAsync(result, cancellationToken);
                }
                while (next != this.lastGrouping);
            }
            await yielder.BreakAsync(cancellationToken);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq
{
    internal abstract class AsyncOrderedEnumerable<TElement> : IAsyncOrderedEnumerable<TElement>
    {
        internal IAsyncEnumerable<TElement> source;

        public IAsyncOrderedEnumerable<TElement> CreateAsyncOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, 
            IComparer<TKey> comparer, 
            bool descending)
        {
            return this.InternalCreateAsyncOrderedEnumerable(keySelector, comparer, descending);
        }

        public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, 
            IComparer<TKey> comparer, 
            bool descending)
        {
            return this.InternalCreateAsyncOrderedEnumerable(keySelector, comparer, descending);
        }

        public IAsyncEnumerator<TElement> GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(
            EnumerableSorter<TElement> next);

        private IAsyncOrderedEnumerable<TElement> InternalCreateAsyncOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer,
            bool descending)
        {
            return new AsyncOrderedEnumerable<TElement, TKey>(
                this.source, keySelector, comparer, descending)
            {
                parent = this
            };
        }

        private IAsyncEnumerator<TElement> InternalGetAsyncEnumerator()
        {
            return AsyncEnumerable.CreateBufferred<TElement>(this.StateMachineIterator)
                .GetAsyncEnumerator();
        }

        private async Task StateMachineIterator(
            IAsyncYielder<TElement> yielder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await this.source.AnyAsync(cancellationToken))
            {
                await yielder.BreakAsync(cancellationToken);
            }

            var buffer = await this.source.ToArrayAsync(cancellationToken);
            var enumerableSorter = this.GetEnumerableSorter(null);
            var array = enumerableSorter.Sort(buffer, buffer.Length);

            int num;
            for (int i = 0; i < buffer.Length; i = num + 1)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = buffer[array[i]];
                await yielder.ReturnAsync(item, cancellationToken);
                num = i;
            }
            array = null;

            await yielder.BreakAsync(cancellationToken);
        }
    }

    internal class AsyncOrderedEnumerable<TElement, TKey> : AsyncOrderedEnumerable<TElement>
    {
        internal AsyncOrderedEnumerable<TElement> parent;

        internal Func<TElement, TKey> keySelector;

        internal IComparer<TKey> comparer;

        internal bool descending;

        internal AsyncOrderedEnumerable(
            IAsyncEnumerable<TElement> source, 
            Func<TElement, TKey> keySelector, 
            IComparer<TKey> comparer, 
            bool descending)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            this.source = source;
            this.parent = null;
            this.keySelector = keySelector;
            this.comparer = comparer ?? Comparer<TKey>.Default;
            this.descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(
            EnumerableSorter<TElement> next)
        {
            EnumerableSorter<TElement> enumerableSorter = 
                new EnumerableSorter<TElement, TKey>(
                    this.keySelector, this.comparer, this.descending, next);

            if (this.parent != null)
            {
                enumerableSorter = this.parent.GetEnumerableSorter(enumerableSorter);
            }

            return enumerableSorter;
        }
    }
}

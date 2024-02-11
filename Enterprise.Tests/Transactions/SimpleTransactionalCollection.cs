using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Transactions;
using Enterprise.Core.Transactions;

namespace Enterprise.Tests.Transactions
{
    internal sealed class SimpleTransactionalCollection<T> : ICollection<T>, IReadOnlyList<T>
    {
        private readonly IInvoker invoker = TransactionalInvoker.Instance;

        private readonly IList<T> items = new ObservableCollection<T>();

        public T this[int index]
        {
            get { return this.items[index]; }
        }

        public int Count
        {
            get { return this.items.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.items.IsReadOnly; }
        }

        public void Add(
            T item)
        {
            if (Transaction.Current == null)
            {
                this.items.Add(item);
                return;
            }

            var command = new AddCommand(this.items, item);
            this.invoker.ExecuteCommand(command);
        }

        public void Clear()
        {
            var clone = new T[this.Count];
            this.CopyTo(clone, 0);

            foreach (var item in clone)
            {
                this.Remove(item);
            }
        }

        public bool Contains(
            T item)
        {
            return this.Contains(item);
        }

        public void CopyTo(
            T[] array, 
            int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public bool Remove(
            T item)
        {
            if (Transaction.Current == null)
            {
                return this.items.Remove(item);
            }

            var command = new RemoveCommand(this.items, item);
            this.invoker.ExecuteCommand(command);

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class AddCommand : ICommand
        {
            private readonly ICollection<T> collection;

            private readonly T item;

            public AddCommand(
                ICollection<T> collection, 
                T item)
            {
                this.collection = collection;
                this.item = item;
            }

            public void Execute()
            {
                this.collection.Add(item);
            }

            public void UnExecute()
            {
                this.collection.Remove(item);
            }
        }

        private class RemoveCommand : ICommand
        {
            private readonly ICollection<T> collection;

            private readonly T item;

            public RemoveCommand(
                ICollection<T> collection,
                T item)
            {
                this.collection = collection;
                this.item = item;
            }

            public void Execute()
            {
                this.collection.Remove(item);
            }

            public void UnExecute()
            {
                this.collection.Add(item);
            }
        }
    }
}

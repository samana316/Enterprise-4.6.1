using System;
using System.Collections;
using System.Collections.Generic;
using Enterprise.Core.Resources;

namespace Enterprise.Core.Linq
{
    partial class AsyncLookup<TKey, TElement>
    {
        internal partial class AsyncGrouping : IAsyncGrouping<TKey, TElement>, IList<TElement>
        {
            internal TKey key;

            internal int hashCode;

            internal TElement[] elements;

            internal int count;

            internal AsyncGrouping hashNext;

            internal AsyncGrouping next;

            public TKey Key
            {
                get
                {
                    return this.key;
                }
            }

            int ICollection<TElement>.Count
            {
                get
                {
                    return this.count;
                }
            }

            bool ICollection<TElement>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            TElement IList<TElement>.this[int index]
            {
                get
                {
                    if (index < 0 || index >= this.count)
                    {
                        throw Error.ArgumentOutOfRange("index");
                    }
                    return this.elements[index];
                }
                set
                {
                    throw Error.NotSupported();
                }
            }

            internal void Add(TElement element)
            {
                if (this.elements.Length == this.count)
                {
                    Array.Resize(ref this.elements, checked(this.count * 2));
                }
                this.elements[this.count] = element;
                this.count++;
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                int num;
                for (int i = 0; i < this.count; i = num + 1)
                {
                    yield return this.elements[i];
                    num = i;
                }
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void ICollection<TElement>.Add(TElement item)
            {
                throw Error.NotSupported();
            }

            void ICollection<TElement>.Clear()
            {
                throw Error.NotSupported();
            }

            bool ICollection<TElement>.Contains(TElement item)
            {
                return Array.IndexOf<TElement>(this.elements, item, 0, this.count) >= 0;
            }

            void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
            {
                Array.Copy(this.elements, 0, array, arrayIndex, this.count);
            }

            bool ICollection<TElement>.Remove(TElement item)
            {
                throw Error.NotSupported();
            }

            int IList<TElement>.IndexOf(TElement item)
            {
                return Array.IndexOf<TElement>(this.elements, item, 0, this.count);
            }

            void IList<TElement>.Insert(int index, TElement item)
            {
                throw Error.NotSupported();
            }

            void IList<TElement>.RemoveAt(int index)
            {
                throw Error.NotSupported();
            }
        }
    }
}

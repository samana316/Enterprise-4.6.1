using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq
{
    partial class AsyncLookup<TKey, TElement>
    {
        partial class AsyncGrouping
        {
            public IAsyncEnumerator<TElement> GetAsyncEnumerator()
            {
                return AsyncEnumerable.CreateBufferred<TElement>(this.StateMachineIterator)
                    .GetAsyncEnumerator();
            }

            IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
            {
                return this.GetAsyncEnumerator();
            }

            private async Task StateMachineIterator(
                IAsyncYielder<TElement> yielder, 
                CancellationToken cancellationToken)
            {
                int num;
                for (int i = 0; i < this.count; i = num + 1)
                {
                    await yielder.ReturnAsync(this.elements[i], cancellationToken);
                    num = i;
                }
                await yielder.BreakAsync(cancellationToken);
            }
        }
    }
}

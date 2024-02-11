using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Linq;

namespace TestWcf
{
    [ServiceBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    internal sealed class RemoteAsyncIterator : DisposableBase, IRemoteAsyncIterator
    {
        private static readonly IAsyncEnumerable<int> source = AsyncEnumerable.Range(1, 10);

        private static IAsyncEnumerator<int> enumerator = source.GetAsyncEnumerator();

        [OperationBehavior]
        public new void Dispose()
        {
            this.Dispose(true);
        }

        [OperationBehavior]
        public int GetCurrent()
        {
            return enumerator.Current;
        }

        [OperationBehavior]
        public Task<bool> MoveNextAsync()
        {
            return enumerator.MoveNextAsync();
        }

        [OperationBehavior]
        public void Reset()
        {
            this.Dispose(true);
            enumerator = source.GetAsyncEnumerator();
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing && enumerator != null)
            {
                enumerator.Dispose();
            }
        }
    }
}

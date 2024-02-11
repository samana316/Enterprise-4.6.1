using System.ServiceModel;
using System.Threading.Tasks;

namespace TestWcf
{
    [ServiceContract]
    public interface IRemoteAsyncIterator
    {
        [OperationContract]
        int GetCurrent();

        [OperationContract]
        Task<bool> MoveNextAsync();

        [OperationContract]
        void Dispose();

        [OperationContract]
        void Reset();
    }
}

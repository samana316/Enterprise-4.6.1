using System.ServiceModel;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading;
using Enterprise.Core.Linq;
using TestWcf;

namespace Enterprise.Tests.Linq.Helpers
{
    public static class RemoteAsyncIterator
    {
        private static readonly object sink = new object();

        public static IAsyncEnumerable<int> Create()
        {
            return AsyncEnumerable.Create<int>(StateMachineIterator);
        }

        private static async Task StateMachineIterator(
            IAsyncYielder<int> yielder)
        {
            var channel = ChannelFactory<IRemoteAsyncIterator>.CreateChannel(
                new BasicHttpBinding(),
                new EndpointAddress("http://localhost:49815/RemoteAsyncIterator.svc"));

            try
            {
                using (await AsyncLock.LockAsync(sink))
                {
                    channel.Reset();
                    while (await channel.MoveNextAsync())
                    {
                        await yielder.ReturnAsync(channel.GetCurrent());
                    }

                    await yielder.BreakAsync();
                    channel.Reset();
                }
            }
            finally
            {
                channel.Dispose();
            }
        }
    }
}

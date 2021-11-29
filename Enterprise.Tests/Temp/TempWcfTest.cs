using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class TempWcfTest
    {
        private const string BaseAddress = "http://localhost/MyService";

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(30000)]
        public async Task TempWcfTestMethod()
        {
            try
            {
                var values = new List<int>();
                
                using (var host = await this.StartHostAsync(CancellationToken.None))
                {
                    Func<IMyService, bool> operation = channel => channel.DoSomething(ref values);

                    var result = operation.Invoke(this.CreateChannelFactory);
                    Trace.WriteLine(result);
                    Assert.IsTrue(result);
                }

                Trace.WriteLine(values.Count);
                Assert.IsTrue(values.Count > 0);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private ChannelFactory<IMyService> CreateChannelFactory()
        {
            return new ChannelFactory<IMyService>(this.CreateDefaultBinding(), BaseAddress);
        }

        private Binding CreateDefaultBinding()
        {
            return new BasicHttpBinding();
        }

        private async Task<IDisposable> StartHostAsync(
            CancellationToken cancellationToken)
        {
            await Task.Yield();

            var host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(
                typeof(IMyService), this.CreateDefaultBinding(), BaseAddress);

            host.Faulted += this.Host_Faulted;
            host.Open();

            return host;
        }

        private void Host_Faulted(
            object sender, 
            EventArgs e)
        {
            Trace.WriteLine(sender, "Faulted");
        }
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        bool DoSomething(ref List<int> values);
    }

    [ServiceBehavior]
    public sealed class MyService : IMyService
    {
        private readonly Random random = new Random();

        [OperationBehavior]
        public bool DoSomething(
            ref List<int> values)
        {
            values.Add(random.Next(0, 9));

            return true;
        }
    }
}

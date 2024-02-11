using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class TaskTest
    {
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task CancelFromAsync()
        {
            try
            {
                Func<CancellationToken, Task> legacy = (ct) =>
                {
                    Action action = () =>
                    {
                        Thread.Sleep(5000);
                    };

                    //return Task.Factory.FromAsync(action.BeginInvoke, action.EndInvoke, null);

                    var tcs = new TaskCompletionSource<object>();

                    action.BeginInvoke(asyncResult =>
                    {
                        ct.ThrowIfCancellationRequested();

                        action.EndInvoke(asyncResult);

                        tcs.SetResult(null);
                    }, null);

                    return tcs.Task;
                };

                var cts = new CancellationTokenSource();
                cts.CancelAfter(1000);

                await Task.Run(async () => await Task.Delay(5000), cts.Token);

                throw new InvalidOperationException("This should throw OperationCanceledException.");
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.IsTrue(exception is OperationCanceledException, exception.Message);
            }
        }
    }
}

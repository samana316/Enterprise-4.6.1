using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class PauseTokenTest
    {
        private readonly PauseTokenSource pts = new PauseTokenSource();

        [TestMethod]
        [TestCategory("PauseToken")]
        [Timeout(10000)]
        public void Main()
        {
            try
            {
                Task.Run(() =>
                 {
                     while (true)
                     {
                         Console.ReadLine();
                         pts.IsPaused = !pts.IsPaused;
                     }
                 });

                SomeMethodAsync(pts.Token).Wait();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("PauseToken")]
        [Timeout(10000)]
        public async Task MainAsync()
        {
            try
            {
                pts.IsPaused = true;

                var longRunningTask = Task.Run(async () =>
                 {
                     while (true)
                     {
                         await Task.Run(() => Trace.Write("|"));
                         pts.IsPaused = !pts.IsPaused;
                     }
                 });

                await SomeMethodAsync(pts.Token);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private async Task SomeMethodAsync(
            PauseToken pauseToken)
        {
            for (int i = 0; i < 10; i++)
            {
                Trace.WriteLine(i);
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                await pauseToken.WaitWhilePausedAsync();
            }
        }
    }
}

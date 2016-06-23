using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class ObservableTest
    {
        [Obsolete]
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest()
        {
            try
            {
                var source = new TempSource();
                
                using (var subscription = source.Subscribe(new AsyncObserver<int>()))
                {
                   await source.StartAsync(CancellationToken.None);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [Obsolete]
        private class TempSource : ObservableBase<int>
        {
            public async Task StartAsync(
                CancellationToken cancellationToken)
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        //await Task.Delay(100);

                        await Task.Run(() => this.OnNext(i), cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                    }
                }

                this.OnCompleted();
            }
        }

        private class AsyncObserver<T> : IObserver<T>
        {
            public async void OnCompleted()
            {
                await Task.Delay(2);
                await Console.Out.WriteLineAsync("OnCompleted");
            }

            public async void OnError(
                Exception error)
            {
                await Console.Out.WriteLineAsync("OnError: " + error);
            }

            public async void OnNext(
                T value)
            {
                await Task.Delay(5);
                await Console.Out.WriteLineAsync("OnNext: " + value);
            }
        }
    }
}

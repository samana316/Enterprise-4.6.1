using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class ForEachAsyncTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ForEachAsyncSimple()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(async (yielder, cancellationToken) => 
                {
                    await yielder.ReturnAsync(1, cancellationToken);
                    await yielder.ReturnAsync(2, cancellationToken);
                    await yielder.ReturnAsync(3, cancellationToken);
                    await yielder.BreakAsync(cancellationToken);
                });

                var sum = 0;
                await source.ForEachAsync(item => sum += item);

                Assert.AreEqual(await source.SumAsync(), sum);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ForEachAsyncDelay()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(async (yielder, cancellationToken) =>
                {
                    await yielder.ReturnAsync(1, cancellationToken);
                    await yielder.ReturnAsync(2, cancellationToken);
                    await yielder.ReturnAsync(3, cancellationToken);
                    await yielder.BreakAsync(cancellationToken);
                });

                var sum = 0;
                await source.ForEachAsync(item => { Task.Delay(100).Wait(); sum += item; });

                Assert.AreEqual(await source.SumAsync(), sum);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

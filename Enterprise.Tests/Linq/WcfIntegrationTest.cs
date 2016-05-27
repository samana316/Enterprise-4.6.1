using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Tests.Linq.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public class WcfIntegrationTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("WhereSelect")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task WcfWhereSelect()
        {
            try
            {
                const int countExpected = 2;

                var source = 
                    from item in RemoteAsyncIterator.Create().Take(3)
                    where item % 2 != 0
                    select Convert.ToDecimal(item);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                Trace.WriteLine("ForEachAsync");
                await source.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await source.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var expected = new[] { 1m, 3m };
                Assert.IsTrue(await source.SequenceEqualAsync(expected, cancellationToken));

                var sum = await source.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await source.AverageAsync(cancellationToken);
                Trace.WriteLine(average, "AverageAsync");

                Assert.AreEqual(sum / countActual, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

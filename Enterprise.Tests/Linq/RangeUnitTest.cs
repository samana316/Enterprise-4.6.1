using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class RangeUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Range")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task RangeSimple()
        {
            try
            {
                const int start = 1;
                const int countExpected = 5;

                var countActual = 0;
                var source = AsyncEnumerable.Range(start, countExpected);

                using (var enumerator = source.GetAsyncEnumerator())
                {
                    var expected = start;
                    while (await enumerator.MoveNextAsync())
                    {
                        var current = enumerator.Current;
                        Trace.WriteLine(current, "MoveNextAsync");
                        Assert.AreEqual(expected, current);

                        countActual++;
                        expected++;
                    }
                }

                Trace.WriteLine(countActual, "Count");
                Assert.AreEqual(countExpected, countActual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Range")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task RangeComplex()
        {
            try
            {
                const int start = 1;
                const int countExpected = 5;
                
                var source = AsyncEnumerable.Range(start, countExpected);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                await source.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await source.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var expected = Enumerable.Range(start, countExpected);
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

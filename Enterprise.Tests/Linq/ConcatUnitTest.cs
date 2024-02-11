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
    public sealed class ConcatUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Concat")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ConcatSimple()
        {
            try
            {
                var first = AsyncEnumerable.Range(1, 3);
                var second = AsyncEnumerable.Range(4, 3);

                var result = first.Concat(second);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                var expected = Enumerable.Range(1, 6);

                Assert.IsTrue(await result.SequenceEqualAsync(expected));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Concat")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ConcatComplex()
        {
            try
            {
                var first = AsyncEnumerable.Range(1, 5);
                var second = AsyncEnumerable.Range(6, 5);

                var result = 
                    from item in first.Concat(second)
                    where item % 2 != 0
                    select Convert.ToDecimal(item) + 0.5m;

                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(10000);

                    var cancellationToken = cts.Token;
                    await result.ForEachAsync(
                        item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                    var expected = new[] { 1.5m, 3.5m, 5.5m, 7.5m, 9.5m };

                    Assert.IsTrue(await result.SequenceEqualAsync(expected, cancellationToken));

                    var count = await result.CountAsync(cancellationToken);
                    Trace.WriteLine(count, "CountAsync");
                    Assert.AreEqual(expected.Count(), count);

                    var sum = await result.SumAsync(cancellationToken);
                    Trace.WriteLine(sum, "SumAsync");

                    var average = await result.AverageAsync(cancellationToken);
                    Trace.Write(average, "AverageAsync");
                    Assert.AreEqual(sum / count, average);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

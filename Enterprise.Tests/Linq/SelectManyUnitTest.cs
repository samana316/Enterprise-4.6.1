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
    public sealed class SelectManyUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SelectMany")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task SelectManySimple()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));

                var source = (new[]
                {
                    AsyncEnumerable.Range(1, 3),
                    AsyncEnumerable.Range(4, 3),
                    AsyncEnumerable.Range(7, 3)
                }).AsAsyncEnumerable();

                var result =
                    from item in source
                    from subItem in item
                    select subItem;

                Assert.IsNotNull(result);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));
                Assert.AreEqual(9, await result.CountAsync(cancelSource.Token));

                var expected = AsyncEnumerable.Range(1, 9);
                Assert.IsTrue(await result.SequenceEqualAsync(expected, cancelSource.Token));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SelectMany")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task SelectManyComplex()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));

                var source = AsyncEnumerable.Create<IAsyncEnumerable<int>>(async (y, ct) =>
                {
                    await y.ReturnAsync(AsyncEnumerable.Range(1, 3), ct);
                    await y.ReturnAsync(AsyncEnumerable.Range(4, 3), ct);
                    await y.ReturnAsync(AsyncEnumerable.Range(7, 3), ct);
                });

                var result =
                    from item in source
                    from subItem in item
                    where subItem % 2 == 0
                    select Convert.ToDecimal(subItem);

                Assert.IsNotNull(result);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                var cancellationToken = cts.Token;

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                const int countExpected = 4;
                var countActual = await result.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var sum = await result.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await result.AverageAsync(cancellationToken);
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

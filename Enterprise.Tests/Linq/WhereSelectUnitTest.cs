using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class WhereSelectUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("WhereSelect")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task WhereSelectSimple()
        {
            try
            {
                var source = AsyncEnumerable.Range(1, 10);

                var result =
                    from item in source
                    where item % 2 == 0
                    select Convert.ToDouble(item);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                var expected = new double[] { 2, 4, 6, 8, 10 };

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
        [TestCategory("WhereSelect")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task WhereSelectComplex()
        {
            try
            {
                var source = AsyncEnumerable.Range(1, 10);

                var result =
                    from item in source
                    where item % 2 == 0
                    select Convert.ToDouble(item);

                const int countExpected = 5;

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                var cancellationToken = cts.Token;

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

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

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("WhereSelect")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task WhereSelectInfinite()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(async (y, ct) =>
                {
                    var condition = true;

                    for (var i = 1; condition; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        await y.ReturnAsync(i, ct);
                    }

                    await y.BreakAsync(ct);
                });

                var result = (
                    from item in source
                    where item <= 5
                    select Convert.ToDouble(item)).Take(5);

                const int countExpected = 5;

                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);
                var cancellationToken = cts.Token;

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

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

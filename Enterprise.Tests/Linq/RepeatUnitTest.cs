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
    public sealed class RepeatUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Repeat")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task RepeatSimple()
        {
            try
            {
                const int element = 1;
                const int countExpected = 5;

                var countActual = 0;
                var source = AsyncEnumerable.Repeat(element, countExpected);

                using (var enumerator = source.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        var current = enumerator.Current;
                        Trace.WriteLine(current, "MoveNextAsync");
                        Assert.AreEqual(element, current);

                        countActual++;
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
        [TestCategory("Repeat")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task RepeatComplex()
        {
            try
            {
                const int element = 1;
                const int countExpected = 5;
                
                var source = AsyncEnumerable.Repeat(element, countExpected);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                await source.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await source.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var expected = Enumerable.Repeat(element, countExpected);
                Assert.IsTrue(await source.SequenceEqualAsync(expected, cancellationToken));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

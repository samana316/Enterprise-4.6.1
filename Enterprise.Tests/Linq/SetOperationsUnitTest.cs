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
    public sealed class SetOperationsUnitTest
    {
        private readonly IAsyncEnumerable<int> first = AsyncEnumerable.Range(1, 4);

        private readonly IAsyncEnumerable<int> second = AsyncEnumerable.Range(3, 4);

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task Distinct()
        {
            try
            {
                var source = (new[] { 1, 2, 1, 2, 3, 1, 2, 1, 2, 1 }).AsAsyncEnumerable();

                var result = source.Distinct();
                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                Assert.AreEqual(3, await result.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task Except()
        {
            try
            {
                var result = first.Except(second);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                Assert.IsTrue(await result.SequenceEqualAsync(new[] { 1, 2 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task Intersect()
        {
            try
            {
                var result = first.Intersect(second);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                Assert.IsTrue(await result.SequenceEqualAsync(new[] { 3, 4 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task Union()
        {
            try
            {
                var result = first.Union(second);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                Assert.IsTrue(await result.SequenceEqualAsync(Enumerable.Range(1, 6)));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

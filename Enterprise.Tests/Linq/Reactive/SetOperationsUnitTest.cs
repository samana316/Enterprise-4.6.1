using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class SetOperationsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task DefaultIfEmptySimple()
        {
            try
            {
                var source1 = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(0);
                });

                var source2 = AsyncObservable.Empty<long>();

                var query1 = source1.DefaultIfEmpty();
                var query2 = source2.DefaultIfEmpty();

                var sequenceEqual = false;
                await query1.SequenceEqual(query2).ForEachAsync(x => sequenceEqual = x);
                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task DistinctSimple()
        {
            try
            {
                var enumerable = new[] { 1, 2, 1, 2, 3, 1, 2, 1, 2, 1 };
                var observable = enumerable.ToAsyncObservable();
                var distinct = observable.Distinct();
                
                using (await distinct.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var sequenceEqual = false;
                await distinct.SequenceEqual(enumerable.Distinct()).ForEachAsync(x => sequenceEqual = x);
                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SetOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task DistinctUntilChangedSimple()
        {
            try
            {
                var enumerable = new[] { 1, 2, 2, 3, 3, 3, 2, 1 };
                var observable = enumerable.ToAsyncObservable();
                var query = observable.DistinctUntilChanged();

                var observer = new TestAsyncObserver<int>();
                using (await query.SubscribeAsync(observer)) { }

                Assert.IsTrue(
                    await observer.Items.SequenceEqualAsync(new[] { 1, 2, 3, 2, 1 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

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
    public sealed class PagingOperationsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Paging")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TakeSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                });

                var take = observable.Take(2);

                using (await take.SubscribeAsync(new TestAsyncObserver<long>())) { }

                var sequenceEqual = false;
                await take.SequenceEqual(new long[] { 1, 2 }).ForEachAsync(x => sequenceEqual = x);
                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Paging")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TakeInfinite()
        {
            try
            {
                var observable = AsyncObservable.Create<int>(async observer =>
                {
                    while (true)
                    {
                        await observer.OnNextAsync(0);
                    }
                });

                var take = observable.Take(2);

                using (await take.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var sequenceEqual = false;
                await take.SequenceEqual(Enumerable.Repeat(0, 2)).ForEachAsync(x => sequenceEqual = x);
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
        [TestCategory("Paging")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TakeWhileSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                });

                var take = observable.TakeWhile(x => x < 3);

                using (await take.SubscribeAsync(new TestAsyncObserver<long>())) { }

                var sequenceEqual = false;
                await take.SequenceEqual(new long[] { 1, 2 }).ForEachAsync(x => sequenceEqual = x);
                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Paging")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TakeWhileInfinite()
        {
            try
            {
                var observable = AsyncObservable.Create<int>(async observer =>
                {
                    var i = 0;
                    while (true)
                    {
                        i++;
                        await observer.OnNextAsync(i);
                    }
                });

                var take = observable.TakeWhile(x => x < 3);

                using (await take.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var sequenceEqual = false;
                await take.SequenceEqual(Enumerable.Range(1, 2)).ForEachAsync(x => sequenceEqual = x);
                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

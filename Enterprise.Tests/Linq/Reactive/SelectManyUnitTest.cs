using System;
using System.Collections.Generic;
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
    public sealed class SelectManyUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SelectMany")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task SelectManySimple()
        {
            try
            {
                var source = AsyncObservable.Create<IAsyncObservable<int>>(async observer =>
                {
                    await observer.OnNextAsync(AsyncObservable.Return(1));
                    await observer.OnNextAsync(AsyncObservable.Return(2));
                    await observer.OnNextAsync(AsyncObservable.Return(3));
                });
                Trace.WriteLine(source, "observable");
                using (await source.SubscribeAsync(new TestAsyncObserver<IAsyncObservable<int>>())) { }

                var query =
                    from collection in source
                    from item in collection
                    select item;
                Trace.WriteLine(query, "query");

                using (await query.SubscribeAsync(new TestAsyncObserver<int>())) { }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SelectMany")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task SelectManyComplex()
        {
            try
            {
                var source = AsyncObservable.Create<IEnumerable<int>>(async observer =>
                {
                    await observer.OnNextAsync(new[] { 1, 2, 3 });
                    await observer.OnNextAsync(Enumerable.Range(4, 3));
                    await observer.OnNextAsync(AsyncEnumerable.Range(7, 3));
                });
                Trace.WriteLine(source, "observable");
                using (await source.SubscribeAsync(new TestAsyncObserver<IEnumerable<int>>())) { }

                var query =
                    from collection in source
                    from item in collection
                    select item;
                Trace.WriteLine(query, "query");
                using (await query.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var count = 0;
                var sum = 0d;
                var average = 0d;

                await query.Count().ForEachAsync(x => count = x);
                Trace.WriteLine(count, "Count");

                await query.Sum().ForEachAsync(x => sum = x);
                Trace.WriteLine(sum, "Sum");

                await query.Average().ForEachAsync(x => average = x);
                Trace.WriteLine(average, "Average");

                Assert.AreEqual(sum / count, average);

                var sequenceEqual = false;
                await query.SequenceEqual(Enumerable.Range(1, 9)).ForEachAsync(x => sequenceEqual = x);

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
        [TestCategory("SelectMany")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task SelectManyNested()
        {
            try
            {
                var source = AsyncObservable.Create<IObservable<int>>(async observer =>
                {
                    await observer.OnNextAsync(AsyncObservable.Repeat(1, 1));
                    await observer.OnNextAsync(AsyncObservable.Return(2));
                    await observer.OnNextAsync(AsyncObservable.Empty<int>());
                    await observer.OnNextAsync(AsyncObservable.Range(3, 4));
                });
                Trace.WriteLine(source, "observable");
                using (await source.SubscribeAsync(new TestAsyncObserver<IObservable<int>>())) { }

                var query =
                    from collection in source
                    from item in collection
                    select item;
                Trace.WriteLine(query, "query");
                using (await query.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var count = 0;
                var sum = 0d;
                var average = 0d;

                await query.Count().ForEachAsync(x => count = x);
                Trace.WriteLine(count, "Count");

                await query.Sum().ForEachAsync(x => sum = x);
                Trace.WriteLine(sum, "Sum");

                await query.Average().ForEachAsync(x => average = x);
                Trace.WriteLine(average, "Average");

                Assert.AreEqual(sum / count, average);

                var sequenceEqual = false;
                await query.SequenceEqual(Enumerable.Range(1, 9)).ForEachAsync(x => sequenceEqual = x);

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

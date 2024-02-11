using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class ConcatUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Concat")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ConcatSimple()
        {
            try
            {
                var first = AsyncObservable.Return(1);
                Trace.WriteLine(first, "first");

                var second = AsyncObservable.Return(2);
                Trace.WriteLine(second, "second");

                var query = first.Concat(second);
                Trace.WriteLine(query, "query");

                var sum = 0;
                await query.ForEachAsync(x => sum += x);

                Assert.AreEqual(3, sum);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Concat")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ConcatComplex()
        {
            try
            {
                var first = AsyncObservable.Return(1);
                Trace.WriteLine(first, "first");

                var second = AsyncObservable.Create<int>(async observer => 
                {
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                });
                Trace.WriteLine(second, "second");

                var third = AsyncObservable.Range(4, 3);
                Trace.WriteLine(third, "third");

                var concat = first.Concat(second).Concat(third);
                Trace.WriteLine(concat, "concat");
                await concat.ForEachAsync(x => Trace.WriteLine(x, "OnNext"));

                var query =
                    from item in concat
                    where item % 2 != 0
                    select Convert.ToDecimal(item) + 0.5m;

                Trace.WriteLine(query, "query");

                var count = 0;
                var sum = 0m;
                var average = 0m;

                await query.Count().ForEachAsync(x => count = x);
                await query.Sum().ForEachAsync(x => sum = x);
                await query.Average().ForEachAsync(x => average = x);

                Trace.WriteLine(count, "Count");
                Trace.WriteLine(sum, "Sum");
                Trace.WriteLine(average, "Average");

                Assert.AreEqual(3, count);
                Assert.AreEqual(10.5m, sum);
                Assert.AreEqual(3.5m, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Concat")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ConcatNested()
        {
            try
            {
                var sources = AsyncObservable.Create<IAsyncObservable<int>>(async observer =>
                {
                    await observer.OnNextAsync(AsyncObservable.Repeat(1, 1));
                    await observer.OnNextAsync(AsyncObservable.Return(2));
                    await observer.OnNextAsync(AsyncObservable.Empty<int>());
                    await observer.OnNextAsync(AsyncObservable.Range(3, 4));
                });
                Trace.WriteLine(sources, "sources");

                var concat = sources.Concat();
                Trace.WriteLine(concat, "concat");
                await concat.ForEachAsync(x => Trace.WriteLine(x, "OnNext"));

                var query =
                    from item in concat
                    where item % 2 != 0
                    select Convert.ToDecimal(item) + 0.5m;

                Trace.WriteLine(query, "query");

                var count = 0;
                var sum = 0m;
                var average = 0m;

                await query.Count().ForEachAsync(x => count = x);
                await query.Sum().ForEachAsync(x => sum = x);
                await query.Average().ForEachAsync(x => average = x);

                Trace.WriteLine(count, "Count");
                Trace.WriteLine(sum, "Sum");
                Trace.WriteLine(average, "Average");

                Assert.AreEqual(3, count);
                Assert.AreEqual(10.5m, sum);
                Assert.AreEqual(3.5m, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

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
    public sealed class SeekOperationsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task ElementAtSimple()
        {
            try
            {
                var start = 1;
                var count = 3;
                var observable = AsyncObservable.Range(start, count);
                var testObserver = new TestAsyncObserver<int>();

                var elementAt1 = observable.ElementAt(1);
                using (await elementAt1.SubscribeAsync(testObserver)) { }

                var elementAt2 = observable.ElementAt(2);
                using (await elementAt2.SubscribeAsync(testObserver)) { };

                var elementAt3 = observable.ElementAt(3);
                using (await elementAt3.SubscribeAsync(testObserver)) { };

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new[] { 2, 3 }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is ArgumentOutOfRangeException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task ElementAtComplex()
        {
            try
            {
                var start = 1;
                var count = 3;
                var observable = AsyncObservable.Range(start, count);

                var elementAts =
                    from item in observable
                    select observable.ElementAt(item);

                var elementAtOrDefaults =
                    from item in observable
                    select observable.ElementAtOrDefault(item);

                var concat = elementAtOrDefaults.Concat(elementAts);

                var testObserver = new TestAsyncObserver<int>();

                await concat.ForEachAsync(async source =>
                {
                    using (await source.SubscribeAsync(testObserver)) { }
                });

                Assert.IsTrue(
                 await testObserver.Items.SequenceEqualAsync(new[] { 2, 3, 0, 2, 3 }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is ArgumentOutOfRangeException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ElementAtInfinite()
        {
            try
            {
                var observable = AsyncObservable.Repeat(1).Select((item, index) => item + index);
                var testObserver = new TestAsyncObserver<int>();

                var elementAts =
                    from x in AsyncEnumerable.Range(0, 3)
                    select observable.ElementAt(x);

                using (await elementAts.Concat().SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new[] { 1, 2, 3 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task FirstSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<bool?>(async observer => 
                {
                    await observer.OnNextAsync(false);
                    await observer.OnNextAsync(true);
                });

                Func<bool?, bool> shouldMatch = x => x.GetValueOrDefault();
                Func<bool?, bool> shouldNotMatch = x => !x.HasValue;

                var query = new[]
                {
                    observable.FirstOrDefault(),
                    observable.FirstOrDefault(shouldMatch),
                    observable.FirstOrDefault(shouldNotMatch),
                    observable.First(),
                    observable.First(shouldMatch),
                    observable.First(shouldNotMatch)
                }.Concat();

                var testObserver = new TestAsyncObserver<bool?>();

                using (await query.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new bool?[] { false, true, null, false, true }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is InvalidOperationException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task FirstInfinite()
        {
            try
            {
                var observable = AsyncObservable.Create<bool?>(async observer =>
                {
                    while (true)
                    {
                        await observer.OnNextAsync(false);
                        await observer.OnNextAsync(true);
                    }
                });

                Func<bool?, bool> predicate = x => x.GetValueOrDefault();

                var query = new[]
                {
                    observable.FirstOrDefault(),
                    observable.FirstOrDefault(predicate),
                    observable.First(),
                    observable.First(predicate),
                }.Concat();

                var testObserver = new TestAsyncObserver<bool?>();

                using (await query.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new bool?[] { false, true, false, true }));

                Assert.IsFalse(
                    await testObserver.Errors.AnyAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task LastSimple()
        {
            try
            {
                var observable = AsyncObservable.Range(1, 5);

                Func<int, bool> shouldMatch = x => x % 2 == 0;
                Func<int, bool> shouldNotMatch = x => x < 0;

                var query = new[]
                {
                    observable.LastOrDefault(shouldMatch),
                    observable.LastOrDefault(shouldNotMatch),
                    observable.Last(),
                    observable.Last(shouldMatch),
                    observable.Last(shouldNotMatch),
                }.Concat();

                var testObserver = new TestAsyncObserver<int>();

                using (await query.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new[] { 4, 0, 5, 4 }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is InvalidOperationException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task LastNested()
        {
            try
            {
                var observable = AsyncObservable.Range(1, 5);
                var query = observable.Last();
                var result = await query;

                Trace.WriteLine(result);
                Assert.AreEqual(5, result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task SingleSimple()
        {
            try
            {
                var source1 = AsyncObservable.Return(1);
                var source2 = AsyncObservable.Range(1, 2);

                Func<int, bool> shouldMatch1 = x => x == 1;
                Func<int, bool> shouldMatch2 = x => x > 1;
                Func<int, bool> shouldMatchBoth = x => x > 0;
                Func<int, bool> shouldNotMatch = x => x < 0;

                var query1 = new[]
                {
                    source1.Single(),
                    source1.Single(shouldMatch1),
                    source2.Single(shouldMatch2),
                    source2.Single()
                }.Concat();

                var testObserver = new TestAsyncObserver<int>();

                using (await query1.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new[] { 1, 1, 2 }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is InvalidOperationException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        public async Task SingleOrDefaultSimple()
        {
            try
            {
                var source1 = AsyncObservable.Return(1);
                var source2 = AsyncObservable.Range(1, 2);

                Func<int, bool> shouldMatch1 = x => x == 1;
                Func<int, bool> shouldMatch2 = x => x > 1;
                Func<int, bool> shouldMatchBoth = x => x > 0;
                Func<int, bool> shouldNotMatch = x => x < 0;

                var query1 = new[]
                {
                    source1.SingleOrDefault(),
                    source1.SingleOrDefault(shouldMatch1),
                    source2.SingleOrDefault(shouldMatch2),
                    source2.SingleOrDefault(shouldNotMatch),
                    source2.SingleOrDefault(shouldMatchBoth)
                }.Concat();

                var testObserver = new TestAsyncObserver<int>();

                using (await query1.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new[] { 1, 1, 2, 0 }));

                Assert.IsTrue(
                    await testObserver.Errors.SingleOrDefaultAsync() is InvalidOperationException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

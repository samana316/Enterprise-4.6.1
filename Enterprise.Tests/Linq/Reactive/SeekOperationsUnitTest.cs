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
    }
}

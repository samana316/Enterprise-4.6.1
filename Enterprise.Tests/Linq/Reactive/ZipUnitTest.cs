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
    public sealed class ZipUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Zip")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ZipSimple()
        {
            try
            {
                var first = AsyncObservable.Range(1, 3);
                var second = AsyncObservable.Range(1, 3);

                var zip = first.Zip(second, (x, y) => x - y);

                var testObserver = new TestAsyncObserver<int>();
                using (await zip.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.AllAsync(x => x == 0));

                Assert.AreEqual(3, await testObserver.Items.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Zip")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ZipSimpleE()
        {
            try
            {
                var first = AsyncObservable.Range(1, 3);
                var second = AsyncEnumerable.Range(1, 3);

                var zip = first.Zip(second, (x, y) => x - y);

                var testObserver = new TestAsyncObserver<int>();
                using (await zip.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.AllAsync(x => x == 0));

                Assert.AreEqual(3, await testObserver.Items.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Zip")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ZipInfinite()
        {
            try
            {
                var first = AsyncObservable.Range(1, 3);
                var second = AsyncObservable.Create<int>(async (observer) =>
                {
                    var i = 0;
                    while (true)
                    {
                        i++;
                        await observer.OnNextAsync(i);
                    }
                });

                var zip = first.Zip(second, (x, y) => x - y);

                var testObserver = new TestAsyncObserver<int>();
                using (await zip.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.AllAsync(x => x == 0));

                Assert.AreEqual(3, await testObserver.Items.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

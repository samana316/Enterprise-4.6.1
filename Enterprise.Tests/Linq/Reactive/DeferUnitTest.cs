using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class DeferUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive.Infinte")]
        [TestCategory("Defer")]
        [TestCategory("Timer")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TimerDeferSimple()
        {
            try
            {
                var source = AsyncObservable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));
                var query = source.Defer();
                var observer = new TestAsyncObserver<long>();

                var subscription = await query.SubscribeAsync(observer);

                await Task.Delay(400);

                subscription.Dispose();

                var count = await observer.Items.CountAsync();

                Assert.IsTrue(count > 3);

                await Task.Delay(200);

                Assert.AreEqual(
                    count, await observer.Items.CountAsync(), "Observer is still subscribed.");
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinte")]
        [TestCategory("Defer")]
        [TestCategory("Timer")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TimerDeferMultipleObservers()
        {
            try
            {
                var source = AsyncObservable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));
                var query = source.Defer();
                var observer1 = new TestAsyncObserver<long>();
                var observer2 = new TestAsyncObserver<long>();

                var subscription1 = await query.SubscribeAsync(observer1);

                await Task.Delay(200);

                var subscription2 = await query.SubscribeAsync(observer2);

                await Task.Delay(200);

                subscription1.Dispose();

                await Task.Delay(200);

                subscription2.Dispose();

                await Task.Delay(200);

                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new long[] { 1, 2, 3, 4 }));

                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new long[] { 3, 4, 5, 6 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

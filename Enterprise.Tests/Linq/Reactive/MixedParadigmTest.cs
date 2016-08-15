using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Core.Linq.Reactive.Subjects;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class MixedParadigmTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Amb")]
        [TestCategory("Unit")]
        public async Task AmbSimple()
        {
            try
            {
                var observer = new TestAsyncObserver<int>();

                var first = AsyncObservable.Range(1, 3).Delay(TimeSpan.FromSeconds(2));
                var second = AsyncObservable.Range(4, 3).Delay(TimeSpan.FromSeconds(1));

                var amb = AsyncObservable.Amb(first, second);

                using (await amb.SubscribeAsync(observer)) { }

                Assert.IsTrue(
                    await observer.Items.SequenceEqualAsync(AsyncEnumerable.Range(4, 3)));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Amb")]
        [TestCategory("Unit")]
        public async Task AmbMultipleSources()
        {
            try
            {
                var observer = new TestAsyncObserver<int>();

                var first = AsyncObservable.Range(1, 3).Delay(TimeSpan.FromSeconds(3));
                var second = AsyncObservable.Range(4, 3).Delay(TimeSpan.FromSeconds(2));
                var third = AsyncObservable.Range(7, 3).Delay(TimeSpan.FromSeconds(1));

                var amb = AsyncObservable.Amb(first, second, third);

                using (await amb.SubscribeAsync(observer)) { }

                Assert.IsTrue(
                    await observer.Items.SequenceEqualAsync(AsyncEnumerable.Range(7, 3)));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Subjects")]
        [TestCategory("Temp")]
        [TestCategory("Unit")]
        public async Task MixSubjectBasedWithAsync()
        {
            try
            {
                var subject = AsyncSubject.Create<int>();

                var query = subject
                    .Where(x => x % 2 == 0);

                var testObserver1 = new TestAsyncObserver<int>();
                var subscription1 = await query.SubscribeRawAsync(testObserver1);

                await subject.OnNextAsync(1);
                await subject.OnNextAsync(2);

                var testObserver2 = new TestAsyncObserver<int>();
                var subscription2 = await query.SubscribeRawAsync(testObserver2);

                await subject.OnNextAsync(3);
                await subject.OnNextAsync(4);

                subscription1.Dispose();

                await subject.OnNextAsync(5);
                await subject.OnNextAsync(6);
                subject.OnCompleted();

                subscription2.Dispose();

                Assert.IsTrue(
                    await testObserver1.Items.SequenceEqualAsync(new[] { 2, 4 }));

                Assert.IsTrue(
                    await testObserver2.Items.SequenceEqualAsync(new[] { 4, 6 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

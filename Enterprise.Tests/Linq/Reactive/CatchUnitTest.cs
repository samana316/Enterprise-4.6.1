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
    public sealed class CatchUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Catch")]
        [TestCategory("Unit")]
        public async Task CatchSimple()
        {
            try
            {
                var first = AsyncObservable.Range(1, 3);
                Trace.WriteLine(first, "first");

                var second = AsyncObservable.Range(4, 3);
                Trace.WriteLine(second, "second");

                var query = first.Catch(second);
                Trace.WriteLine(query, "query");

                var observer1 = new TestAsyncObserver<int>();
                using (await query.SubscribeAsync(observer1)) { }

                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new[] { 1, 2, 3 }));

                var observer2 = new CatchAsyncObserver<int>(x => x == 3);
                using (await query.SubscribeRawAsync(observer2)) { }

                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new[] { 1, 2, 4, 5, 6 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Catch")]
        [TestCategory("Unit")]
        public async Task CatchWithHandler()
        {
            try
            {
                var observer1 = new TestAsyncObserver<int>();
                var observer2 = new CatchAsyncObserver<int>(x => x == 3);

                var source = AsyncObservable.Range(1, 3);
                var handler = new Func<InvalidOperationException, IAsyncObservable<int>>(error =>
                {
                    observer1.OnError(error);

                    return AsyncObservable.Range(4, 3);
                });

                var query = source.Catch(handler);
                Trace.WriteLine(query, "query");

                using (await query.SubscribeAsync(observer1)) { }

                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new[] { 1, 2, 3 }));

                using (await query.SubscribeAsync(observer2)) { }

                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new[] { 1, 2, 4, 5, 6 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Catch")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CatchInfinite()
        {
            try
            {
                var sources = AsyncEnumerable.Create<IAsyncObservable<int>>(
                    async yielder =>
                    {
                        for (var i = 1; true; i++)
                        {
                            var range = AsyncObservable.Range(i, 3);

                            await yielder.ReturnAsync(range);
                        }
                    });

                var query = sources.Catch();
                Trace.WriteLine(query, "query");

                var observer = new CatchAsyncObserver<int>(x => x == 3);
                using (await query.SubscribeRawAsync(observer)) { }

                Assert.IsTrue(
                    await observer.Items.SequenceEqualAsync(new[] { 1, 2, 2, 4, 5, 6 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private sealed class CatchAsyncObserver<T> : TestAsyncObserver<T>
        {
            private readonly Func<T, bool> predicate;

            public CatchAsyncObserver(
                Func<T, bool> predicate)
            {
                this.predicate = predicate;
            }

            public override Task OnNextAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                if (this.predicate(value))
                {
                    throw new InvalidOperationException();
                }

                return base.OnNextAsync(value, cancellationToken);
            }
        }
    }
}

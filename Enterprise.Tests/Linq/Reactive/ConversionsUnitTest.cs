using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class ConversionsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Conversions")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CastSimple()
        {
            try
            {
                var source = AsyncObservable.Create<object>(async observer => 
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2.0);
                    await observer.OnNextAsync("Three");
                });

                var query1 = source.Cast<IConvertible>();
                var observer1 = new TestAsyncObserver<IConvertible>();

                using (await query1.SubscribeAsync(observer1)) { }
                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new IConvertible[] { 1, 2.0, "Three" }));

                var query2 = source.Cast<int>();
                var observer2 = new TestAsyncObserver<int>();

                using (await query2.SubscribeAsync(observer2)) { }
                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new[] { 1 }));
                Assert.IsTrue(
                    await observer2.Errors.SingleOrDefaultAsync() is InvalidCastException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Conversions")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CastInfinite()
        {
            try
            {
                var source = AsyncObservable.Create<object>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2.0);
                    await observer.OnNextAsync("Three");

                    while (true)
                    {
                        await observer.OnNextAsync(null);
                    }
                });

                var query1 = source.Cast<IConvertible>().Take(3);
                var observer1 = new TestAsyncObserver<IConvertible>();

                using (await query1.SubscribeAsync(observer1)) { }
                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new IConvertible[] { 1, 2.0, "Three" }));

                var query2 = source.Cast<int>();
                var observer2 = new TestAsyncObserver<int>();

                using (await query2.SubscribeAsync(observer2)) { }
                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new[] { 1 }));
                Assert.IsTrue(
                    await observer2.Errors.SingleOrDefaultAsync() is InvalidCastException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Conversions")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task OfTypeSimple()
        {
            try
            {
                var source = AsyncObservable.Create<object>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2.0);
                    await observer.OnNextAsync("Three");
                });

                var query1 = source.OfType<IConvertible>();
                var observer1 = new TestAsyncObserver<IConvertible>();

                using (await query1.SubscribeAsync(observer1)) { }
                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new IConvertible[] { 1, 2.0, "Three" }));

                var query2 = source.OfType<double>();
                var observer2 = new TestAsyncObserver<double>();

                using (await query2.SubscribeAsync(observer2)) { }
                Assert.IsTrue(
                    await observer2.Items.SequenceEqualAsync(new[] { 2.0 }));
                Assert.IsFalse(
                    await observer2.Errors.AnyAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Conversions")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ToDictionarySimple()
        {
            try
            {
                var source = AsyncObservable.Create<string>(async observer => 
                {
                    await observer.OnNextAsync("A");
                    await observer.OnNextAsync("AB");
                    await observer.OnNextAsync("ABC");
                });

                var query = source.ToDictionary(x => x.Length);

                await query.ForEachAsync(x => 
                {
                    Trace.WriteLine(JsonConvert.SerializeObject(x, Formatting.Indented));

                    Assert.IsTrue(x.Keys.SequenceEqual(new[] { 1, 2, 3 }));
                });
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

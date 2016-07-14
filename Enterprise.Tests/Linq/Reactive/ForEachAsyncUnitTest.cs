using System;
using System.Collections.Generic;
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
    public sealed class ForEachAsyncUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Unit")]
        public async Task ForEachAsyncError()
        {
            try
            {
                var count = 5;
                var source = AsyncObservable.Range(1, count);

                Action<int> onNext = x =>
                {
                    var y = 1m / (count - x);

                    Trace.WriteLine(y, "OnNext");
                };

                await source.ForEachAsync(onNext);
            }
            catch (AggregateException exception)
            {
                var inner = exception.InnerException;

                Trace.WriteLine(inner);

                Assert.IsTrue(inner is DivideByZeroException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.IsTrue(exception is DivideByZeroException);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ForEachAsyncNested()
        {
            try
            {
                var source = AsyncObservable.Create<IAsyncObservable<int>>(async observer =>
                {
                    await observer.OnNextAsync(AsyncObservable.Repeat(1, 1));
                    await observer.OnNextAsync(AsyncObservable.Return(2));
                    await observer.OnNextAsync(AsyncObservable.Empty<int>());
                    await observer.OnNextAsync(AsyncObservable.Range(3, 4));
                });

                var list = new List<int>();

                Func<int, int, CancellationToken, Task> inner = async (item, index, cancellationToken) => 
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Console.Out.WriteLineAsync("Inner: " + item);
                    list.Add(item);
                };

                Func<IAsyncObservable<int>, int, CancellationToken, Task> outer = async (item, index, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Console.Out.WriteLineAsync("Outer: " + item);
                    await item.ForEachAsync(inner, cancellationToken);
                };

                await source.ForEachAsync(outer);

                Assert.IsTrue(list.SequenceEqual(Enumerable.Range(1, 6)));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ForEachAsyncInfinite()
        {
            var list = new List<int>();

            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(1);

                    var source = AsyncObservable.Repeat(1);

                    await source.ForEachAsync(list.Add, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException exception)
            {
                Trace.WriteLine(exception);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                Trace.WriteLine(list.Count);
            }
        }
    }
}

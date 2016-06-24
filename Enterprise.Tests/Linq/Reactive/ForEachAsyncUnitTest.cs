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
    }
}

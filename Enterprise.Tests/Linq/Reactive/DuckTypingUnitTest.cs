using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class DuckTypingUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("DuckTyping")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GetEnumeratorSimple()
        {
            try
            {
                var source = AsyncObservable.Range(1, 5);

                using (var enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Trace.WriteLine(enumerator.Current, "MoveNext");
                    }
                }

                using (var asyncEnumerator = source.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync())
                    {
                        Trace.WriteLine(asyncEnumerator.Current, "MoveNextAsync");
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("DuckTyping")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GetAwaiterSimple()
        {
            try
            {
                var source = AsyncObservable.Range(1, 5);

                var awaiters = new[]
                {
                    await source,
                    await source,
                    await source,
                    await source,
                    await source
                };

                Trace.WriteLine(JsonConvert.SerializeObject(awaiters));

                Assert.IsTrue(awaiters.SequenceEqual(Enumerable.Repeat(5, 5)));

                await source;
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

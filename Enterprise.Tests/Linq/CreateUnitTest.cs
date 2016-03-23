using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Collections.Extensions;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class CreateUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateSimple()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(
                    async (yielder, cancellationToken) =>
                    {
                        await yielder.ReturnAsync(1, cancellationToken);
                        await yielder.ReturnAsync(2, cancellationToken);
                        await yielder.ReturnAsync(3, cancellationToken);
                    });

                await source.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                var count = await source.CountAsync();
                Trace.WriteLine(count, "CountAsync");
                Assert.AreEqual(3, count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateLoop()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(
                    async (yielder, cancellationToken) =>
                    {
                        for (var i = 1; i <= 5; i++)
                        {
                            if (i % 2 == 0)
                            {
                                await yielder.ReturnAsync(i, cancellationToken);
                            }
                        }
                    });

                await source.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                Assert.AreEqual(2, await source.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateLoopWithTasks()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(
                    async (yielder, cancellationToken) =>
                    {
                        for (var i = 1; i <= 5; await Task.Run(() => i++, cancellationToken))
                        {
                            if (i % 2 == 0)
                            {
                                await yielder.ReturnAsync(i, cancellationToken);
                            }
                        }
                    });

                await source.ForEachAsync(async item =>
                    await Task.Run(() => Trace.WriteLine(item, "MoveNextAsync")));

                var count = await source.CountAsync();
                Trace.WriteLine(count, "CountAsync");
                Assert.AreEqual(2, count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateWithCallStack()
        {
            try
            {
                var source = AsyncEnumerable.Create<int>(
                     async (yielder, cancellationToken) =>
                     {
                         for (var i = 10; i >= 0; i--)
                         {
                             try
                             {
                                 var x = 10 / i;

                                 await yielder.ReturnAsync(x, cancellationToken);
                             }
                             catch (DivideByZeroException exception)
                             {
                                 Trace.WriteLine(exception);
                                 await yielder.ReturnAsync(-1, cancellationToken);
                             }
                         }
                     });

                await source.ForEachAsync(async item =>
                    await Task.Run(() => Trace.WriteLine(item, "MoveNextAsync")));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }


        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateWithWhereSelect()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));

                var cancellationToken = cancelSource.Token;

                var source = AsyncEnumerable.Create<int>(async (y, ct) =>
                {
                    for (var i = 1; i <= 9; i++)
                    {
                        await y.ReturnAsync(i, ct);
                    }
                });

                var result =
                    from x in source
                    where x % 2 == 0
                    select new int?(x);

                Assert.IsNotNull(result);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));
                Assert.AreEqual(4, await result.CountAsync(cancellationToken));

                var expected = new int?[] { 2, 4, 6, 8 };
                Assert.IsTrue(await result.SequenceEqualAsync(expected, cancellationToken));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateWithSelectMany()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));

                var source = AsyncEnumerable.Create<IAsyncEnumerable<int>>(
                    async (yielder, cancellationToken) =>
                    {
                        var inner1 = AsyncEnumerable.Range(1, 3);
                        var inner2 = AsyncEnumerable.Range(4, 3);
                        var inner3 = AsyncEnumerable.Range(7, 3);

                        Trace.WriteLine("await yield ReturnAsync 1");
                        await yielder.ReturnAsync(inner1, cancellationToken);
                        Trace.WriteLine("await yield ReturnAsync 2");
                        await yielder.ReturnAsync(inner2, cancellationToken);
                        Trace.WriteLine("await yield ReturnAsync 3");
                        await yielder.ReturnAsync(inner3, cancellationToken);
                    });

                Trace.WriteLine("Nested MoveNextAsync");
                using (var outer = source.GetAsyncEnumerator())
                {
                    while (await outer.MoveNextAsync(cancelSource.Token))
                    {
                        Trace.WriteLine("Outer Loop");

                        using (var inner = outer.Current.GetAsyncEnumerator())
                        {
                            while (await inner.MoveNextAsync(cancelSource.Token))
                            {
                                await Task.Delay(1);

                                Trace.WriteLine(inner.Current, "Inner Loop");
                            }
                        }
                    }
                }

                Trace.WriteLine("Nested ForEachAsync");
                await source.ForEachAsync(async (group) =>
                {
                    Trace.WriteLine("Outer Loop");

                    await group.ForEachAsync(async (element) =>
                    {
                        await Task.Delay(1);

                        Trace.WriteLine(element, "Inner Loop");
                    },
                    cancelSource.Token);
                },
                cancelSource.Token);

                var result =
                    from item in source
                    from subItem in item
                    select subItem;

                Assert.IsNotNull(result);

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));
                Assert.AreEqual(9, await result.CountAsync(cancelSource.Token));

                var expected =
                    from item in GetFromNormalYield()
                    from subItem in item
                    select subItem;

                expected.ForEach(item => Trace.WriteLine(item, "MoveNext"));

                Assert.IsTrue(await result.SequenceEqualAsync(expected, cancelSource.Token));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private IEnumerable<IEnumerable<int>> GetFromNormalYield()
        {
            yield return Enumerable.Range(1, 3);
            yield return Enumerable.Range(4, 3);
            yield return Enumerable.Range(7, 3);
        }
    }
}

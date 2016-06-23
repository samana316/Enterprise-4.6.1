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
    public sealed class AsyncObservableTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CreateSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                });
                Trace.WriteLine(observable, "observable");

                var testObserver = new TestAsyncObserver<long>();
                using (await observable.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new long[] { 1, 2, 3 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CreateInfinite()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var observable = AsyncObservable.Create<long>(async (observer, ct) =>
                {
                    for (var i = 1; true; i++)
                    {
                        ct.ThrowIfCancellationRequested();

                        await Task.Delay(100, ct);

                        await observer.OnNextAsync(i, ct);
                    }
                });

                Trace.WriteLine(observable, "observable");

                await observable.SubscribeAsync(new TestAsyncObserver<long>(), cancellationToken);
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
                cancellationTokenSource.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Create")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CreateWithWhereSelect()
        {
            try
            {
                var source = AsyncObservable.Create<int>(async observer =>
                {
                    try
                    {
                        var iterable = AsyncEnumerable.Range(1, 5);

                        for (var i = 0; i <= 5; i++)
                        {
                            var item = await iterable.ElementAtAsync(i);

                            await observer.OnNextAsync(item);
                        }
                    }
                    catch (Exception exception)
                    {
                        observer.OnError(exception);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                });
                Trace.WriteLine(source, "source");

                var query =
                    from item in source
                    where item % 2 != 0
                    select Convert.ToDecimal(item);
                Trace.WriteLine(query, "query");

                var testObserver = new TestAsyncObserver<decimal>();
                await query.SubscribeAsync(testObserver);

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new decimal[] { 1, 3, 5 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Count")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task CountSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    try
                    {
                        await observer.OnNextAsync(1);
                        await observer.OnNextAsync(2);
                        await observer.OnNextAsync(3);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                });
                Trace.WriteLine(observable, "observable");

                var count = observable.Count(x => x % 2 != 0);
                Trace.WriteLine(count, "count");

                using (await count.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var result = 0;
                await count.ForEachAsync(x => result += x);
                Assert.AreEqual(2, result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Sum")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task SumSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<float?>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(null);
                    await observer.OnNextAsync(3);
                });
                Trace.WriteLine(observable, "observable");

                var sum = observable.Sum();
                Trace.WriteLine(sum, "sum");

                using (await sum.SubscribeAsync(new TestAsyncObserver<float?>())) { }

                float? result = 0;
                await sum.ForEachAsync(x => result += x);
                Assert.AreEqual(4, result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Sum")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task SumProjected()
        {
            try
            {
                var observable = AsyncObservable.Create<float?>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(null);
                    await observer.OnNextAsync(3);
                });
                Trace.WriteLine(observable, "observable");

                var sum = observable.Sum(x => x.HasValue ? Convert.ToDecimal(x) + 0.1m : default(decimal?));
                Trace.WriteLine(sum, "sum");

                using (await sum.SubscribeAsync(new TestAsyncObserver<decimal?>())) { }

                decimal? result = 0m;
                await sum.ForEachAsync(x => result += x);
                Assert.AreEqual(4.2m, result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Timer")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task TimerSimple()
        {
            try
            {
                var observer = new TestAsyncObserver<long>();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(4000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var source = AsyncObservable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
                    Trace.WriteLine(source, "source");

                    using (var subscription = await source.SubscribeAsync(observer, cancellationToken))
                    {
                    }
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
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Timer")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task TimerWithWhereSelect()
        {
            try
            {
                var observer = new TestAsyncObserver<decimal>();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(5000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var source = AsyncObservable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)).Take(5);
                    Trace.WriteLine(source, "source");

                    var query =
                        from item in source
                        where item % 2 != 0
                        select Convert.ToDecimal(item);
                    Trace.WriteLine(query, "query");

                    using (var subscription = await query.SubscribeAsync(observer, cancellationToken))
                    {
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
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task ToAsyncEnumerableSimple()
        {
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(5000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var observable = AsyncObservable.Create<float?>(async (observer, ct) =>
                    {
                        await observer.OnNextAsync(1, ct);
                        await observer.OnNextAsync(null, ct);
                        await observer.OnNextAsync(3, ct);
                    });

                    var enumerable = observable.ToAsyncEnumerable();

                    var count = await enumerable.CountAsync(cancellationToken);
                    var sum = await enumerable.SumAsync(cancellationToken);
                    var average = await enumerable.AverageAsync(cancellationToken);

                    Trace.WriteLine(count, "Count");
                    Trace.WriteLine(sum, "Sum");
                    Trace.WriteLine(average, "Average");

                    Assert.AreEqual(3, count);
                    Assert.AreEqual(4f, sum);
                    Assert.AreEqual(2f, average);
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
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task ToAsyncObservable()
        {
            try
            {
                var observer = new TestAsyncObserver<long>();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(5000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var source = (from item in AsyncEnumerable.Range(1, 3)
                                  select Convert.ToInt64(item)).ToAsyncObservable();

                    Trace.WriteLine(source, "source");

                    using (var subscription = await source.SubscribeAsync(observer, cancellationToken))
                    {
                    }

                    var toList = source.ToList();
                    await toList.ForEachAsync(list => Trace.WriteLine(list.Count));

                    var sequenceEqual = false;
                    var query = source.SequenceEqual(new long[] { 1, 2, 3 });
                    await query.ForEachAsync(x => sequenceEqual = x);

                    Assert.IsTrue(sequenceEqual);
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
        [TestCategory("WhereSelect")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task WhereSelectSimple()
        {
            try
            {
                var observer = new TestAsyncObserver<double>();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(5000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var source = AsyncEnumerable.Range(1, 5).ToAsyncObservable();
                    Trace.WriteLine(source, "source");

                    var query =
                        from item in source
                        where item % 2 != 0
                        select Convert.ToDouble(item);
                    Trace.WriteLine(query, "query");

                    using (var subscription = await query.SubscribeAsync(observer, cancellationToken))
                    {
                    }

                    var sequenceEqual = false;
                    await query.SequenceEqual(new double[] { 1, 3, 5 }).ForEachAsync(x => sequenceEqual = x);

                    Assert.IsTrue(sequenceEqual);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("WhereSelect")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task WhereSelectInfinite()
        {
            try
            {
                var observer = new TestAsyncObserver<double>();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(2000);
                    var cancellationToken = cancellationTokenSource.Token;

                    var source = AsyncObservable.Create<int>(async (o, ct) => 
                    {
                        var i = 0;
                        while (true)
                        {
                            ct.ThrowIfCancellationRequested();

                            i++;
                            await o.OnNextAsync(i, ct);
                        }
                    });

                    Trace.WriteLine(source, "source");

                    var query =
                        (from item in source
                        where item < 10
                        select Convert.ToDouble(item)).Take(5);
                    Trace.WriteLine(query, "query");

                    using (await query.SubscribeAsync(observer, cancellationToken)) { }

                    var sequenceEqual = false;
                    await query
                        .SequenceEqual(new double[] { 1, 2, 3, 4, 5 })
                        .ForEachAsync(x => sequenceEqual = x);

                    Assert.IsTrue(sequenceEqual);
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
        [TestCategory("Aggregate")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task AggregateSimple()
        {
            try
            {
                var source = AsyncObservable.Create<string>(async observer => 
                {
                    await observer.OnNextAsync("A");
                    await observer.OnNextAsync("B");
                    await observer.OnNextAsync("C");
                    await observer.OnNextAsync("D");
                });
                Trace.WriteLine(source, "source");

                var query = source.Aggregate((x, y) => x + y);
                Trace.WriteLine(query, "query");

                using (await query.SubscribeAsync(new TestAsyncObserver<string>())) { };

                var aggregate = default(string);
                await query.ForEachAsync(x => aggregate = x);

                Assert.AreEqual("ABCD", aggregate);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("All")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task AllSimple()
        {
            try
            {
                var source = AsyncObservable.Create<int>(async observer =>
                {
                    for (var i = 1; i < 5; i++)
                    {
                        await observer.OnNextAsync(i);
                    }
                });
                Trace.WriteLine(source, "source");
                using (await source.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var query1 = source.All(x => x > 0);
                Trace.WriteLine(query1, "query1");

                var all1 = false;
                await query1.ForEachAsync(x => all1 = x);
                Trace.WriteLine(all1, "all1");
                Assert.IsTrue(all1);

                var query2 = source.All(x => x % 2 == 0);
                Trace.WriteLine(query2, "query2");

                var all2 = false;
                await query2.ForEachAsync(x => all2 = x);
                Trace.WriteLine(all2, "all2");
                Assert.IsFalse(all2);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("All")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task AllInfinite()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(5000);

            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var source = AsyncObservable.Create<int>(async (observer, ct) =>
                {
                    for (var i = 1; i < 10; i++)
                    {
                        ct.ThrowIfCancellationRequested();

                        await Task.Delay(100, ct);
                        await observer.OnNextAsync(i, ct);
                    }
                });
                Trace.WriteLine(source, "source");

                var query1 = source.All(x => x < 0);
                Trace.WriteLine(query1, "query1");

                var all1 = false;
                await query1.ForEachAsync(x => all1 = x, cancellationToken);
                Trace.WriteLine(all1, "all1");
                Assert.IsFalse(all1);

                var query2 = source.All(x => x % 2 == 0);
                Trace.WriteLine(query2, "query2");

                var all2 = false;
                await query2.ForEachAsync(x => all2 = x, cancellationToken);
                Trace.WriteLine(all2, "all2");
                Assert.IsFalse(all2);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Any")]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public async Task AnySimple()
        {
            try
            {
                var source = AsyncObservable.Create<int>(async observer =>
                {
                    for (var i = 1; i < 5; i++)
                    {
                        await observer.OnNextAsync(i);
                    }
                });
                Trace.WriteLine(source, "source");
                using (await source.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var results = AsyncObservable.Create<IAsyncObservable<bool>>(async observer =>
                {
                    await observer.OnNextAsync(source.Any());
                    await observer.OnNextAsync(source.Any(x => x == 3));
                    await observer.OnNextAsync(source.Any(x => x < 0));
                    await observer.OnNextAsync(source.Any(x => x > 1));
                });

                var query =
                    from result in results
                    from item in result
                    select item;

                using (await query.SubscribeAsync(new TestAsyncObserver<bool>())) { }

                var sequenceEqual = false;
                await query
                    .SequenceEqual(new[] { true, true, false, true })
                    .ForEachAsync(x => sequenceEqual = x);

                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("Any")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task AnyInfinite()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                cancellationTokenSource.CancelAfter(2000);
                var cancellationToken = cancellationTokenSource.Token;

                var source = AsyncObservable.Create<int>(async observer =>
                {
                    for (var i = 1; true; i++)
                    {
                        await observer.OnNextAsync(i);
                    }
                });
                Trace.WriteLine(source, "source");

                var results = AsyncObservable.Create<IAsyncObservable<bool>>(async observer =>
                {
                    await observer.OnNextAsync(source.Any());
                    await observer.OnNextAsync(source.Any(x => x == 3));
                    await observer.OnNextAsync(source.Any(x => x > 1));
                });

                var query =
                    from result in results
                    from item in result
                    select item;

                using (await query.SubscribeAsync(new TestAsyncObserver<bool>(), cancellationToken)) { }

                var allTrue = false;
                await query.All(x => x).ForEachAsync(x => allTrue = x, cancellationToken);

                Assert.IsTrue(allTrue);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SequenceEqual")]
        [TestCategory("Unit")]
        [Timeout(50000)]
        public async Task SequenceEqualSimple()
        {
            try
            {
                var first = AsyncObservable.Range(1, 3);
                var second = new[] { 1, 2, 3 };

                var sequenceEqual = false;
                var query = first.SequenceEqual(second);
                await query.ForEachAsync(x => sequenceEqual = x);

                Assert.IsTrue(sequenceEqual);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("ToList")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task ToListSimple()
        {
            try
            {
                var source = AsyncObservable.Range(1, 3);
                var toList = source.ToList();

                await toList.ForEachAsync(list => 
                {
                    Trace.WriteLine(list.Count);
                    Assert.AreEqual(3, list.Count);
                });
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("While")]
        [TestCategory("Unit")]
        [Timeout(50000)]
        public async Task WhileSimple()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(2000);

            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var x = 0;
                var source = AsyncObservable.Create<int>(async (observer) =>
                {
                    x = 1;
                    await observer.OnNextAsync(x++);
                    await observer.OnNextAsync(x++);
                    await observer.OnNextAsync(x++);
                    await observer.OnNextAsync(x++);
                    await observer.OnNextAsync(x++);
                    await observer.OnNextAsync(x++);
                });
                Trace.WriteLine(source, "source");

                var query = source.While(() => x <= 5);
                Trace.WriteLine(query, "query");

                using (await query.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var sum = 0;
                await query.Sum().ForEachAsync(y => sum = y);
                Assert.AreEqual(Enumerable.Range(1, 4).Sum(), sum);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.Infinite")]
        [TestCategory("While")]
        [TestCategory("Unit")]
        [Timeout(50000)]
        public async Task WhileInfinite()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(2000);

            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var x = 0;
                var source = AsyncObservable.Create<int>(async (observer, ct) =>
                {
                    x = 0;
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        x++;
                        await observer.OnNextAsync(x, ct);
                    }
                });
                Trace.WriteLine(source, "source");

                var query = source.While(() => x <= 5);
                Trace.WriteLine(query, "query");

                using (await query.SubscribeAsync(new TestAsyncObserver<int>())) { }

                var sum = 0;
                await query.Sum().ForEachAsync(y => sum = y);
                Assert.AreEqual(Enumerable.Range(1, 5).Sum(), sum);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }
    }
}

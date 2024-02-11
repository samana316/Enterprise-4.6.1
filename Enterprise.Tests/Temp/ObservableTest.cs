using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Helpers.Data;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using static System.Reactive.Linq.Observable;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class ObservableTest
    {
        private readonly School school = new School();

        [Obsolete]
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest()
        {
            try
            {
                var source = new TempSource();
                
                using (var subscription = source.Subscribe(new AsyncObserver<int>()))
                {
                   await source.StartAsync(CancellationToken.None);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest2()
        {
            try
            {
                try
                {
                    var source = Range(1, 5);

                    var awaiters = new[]
                    {
                    await source,
                    await source,
                    await source,
                    await source,
                    await source
                };

                    Trace.WriteLine(JsonConvert.SerializeObject(awaiters));

                    Assert.IsTrue(awaiters.SequenceEqual(Enumerable.Range(1, 5)));

                    await source;
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception);

                    Assert.IsTrue(exception is IndexOutOfRangeException, exception.Message);
                }

                await Task.Yield();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest3()
        {
            try
            {
                await Task.Yield();
                var cancellationToken = CancellationToken.None;

                var data = new TestDbDataReader();
                const string command = "SELECT 1 UNION SELECT 2 UNION SELECT 3";
                var timestamp = DateTime.Now;

                var source = Create<IObservable<int>>(async observer =>
                {
                    using (var reader = await data.ExecuteAsync(command, cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var message = string.Format(
                                "{0}, {1}",
                                observer.GetType().Name,
                                (DateTime.Now - timestamp).TotalMilliseconds);

                            await Console.Out.WriteLineAsync(message);
                            var result = Return(reader.GetInt32(0));

                            observer.OnNext(result);
                        }
                    }
                });
                Trace.WriteLine(source.GetType().AssemblyQualifiedName, "Source");

                var query =
                    from child in source
                    from item in child
                    where item % 2 != 0
                    select item.ToString();

                Trace.WriteLine(query.GetType().AssemblyQualifiedName, "Query");

                await query.ForEachAsync(
                    async x => { await Console.Out.WriteLineAsync(x); await Task.Delay(100); }, 
                    cancellationToken);

                var expected = (new[] { "1", "3" }).ToObservable();
                Trace.WriteLine(expected.GetType().AssemblyQualifiedName, "Expected");

                Assert.IsTrue(await query.SequenceEqual(expected));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest4()
        {
            try
            {
                await Task.Yield();
                var cancellationToken = CancellationToken.None;

                var source = Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
                Trace.WriteLine(source.GetType().AssemblyQualifiedName, "Source");

                var query =
                    from item in source
                    where item % 2 != 0
                    select item.ToString();

                query = query.Take(2);
                Trace.WriteLine(query.GetType().AssemblyQualifiedName, "Query");

                await query.ForEachAsync(
                    async x => { await Console.Out.WriteLineAsync(x); await Task.Delay(100); },
                    cancellationToken);

                var expected = (new[] { "1", "3" }).ToObservable();
                Trace.WriteLine(expected.GetType().AssemblyQualifiedName, "Expected");

                Assert.IsTrue(await query.SequenceEqual(expected));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest5()
        {
            try
            {
                await Task.Yield();

                var observable = school.Students.ToObservable();

                var queryNestedGroups =
                    from student in observable
                    group student by student.Year into newGroup1
                    from newGroup2 in
                        (from student in newGroup1
                         group student by student.LastName)
                    group newGroup2 by newGroup1.Key;

                await queryNestedGroups.ForEachAsync(async (outerGroup, ct) =>
                {
                    Console.WriteLine("DataClass.Student Level = {0}", outerGroup.Key);

                    await outerGroup.ForEachAsync(async (innerGroup, ct1) =>
                    {
                        Console.WriteLine("\tNames that begin with: {0}", innerGroup.Key);

                        await innerGroup.ForEachAsync(innerGroupElement =>
                        {
                            Console.WriteLine(
                                "\t\t{0} {1}",
                                innerGroupElement.LastName,
                                innerGroupElement.FirstName);
                        });
                    });
                });
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempObservableTest6()
        {
            try
            {
                await Task.Yield();

                var outer = school.Students.ToObservable();

                var inner = Enum.GetValues(typeof(GradeLevel))
                    .OfType<GradeLevel>()
                    .Select(value => new { Year = value, Description = value.ToString() })
                    .ToObservable();

                var query =
                    from student in outer
                    join grade in inner
                        on Never<int>() equals Never<int>()
                    where student.Year == grade.Year
                    select new { Year = grade.Description, StudentName = student.ToString() };

                await query.ForEachAsync(item => Trace.WriteLine(item, "Item"));

                var countExpected = 12;

                Assert.AreEqual(countExpected, await query.Count());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [Obsolete]
        private class TempSource : ObservableBase<int>
        {
            public async Task StartAsync(
                CancellationToken cancellationToken)
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        //await Task.Delay(100);

                        await Task.Run(() => this.OnNext(i), cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                    }
                }

                this.OnCompleted();
            }
        }

        private class AsyncObserver<T> : IObserver<T>
        {
            public async void OnCompleted()
            {
                await Task.Delay(2);
                await Console.Out.WriteLineAsync("OnCompleted");
            }

            public async void OnError(
                Exception error)
            {
                await Console.Out.WriteLineAsync("OnError: " + error);
            }

            public async void OnNext(
                T value)
            {
                await Task.Delay(5);
                await Console.Out.WriteLineAsync("OnNext: " + value);
            }
        }

        private class CatchAsyncObserver<T> : TestAsyncObserver<T>
        {
            public override void OnNext(
                T value)
            {
                if (Equals(value, 3))
                {
                    throw new InvalidOperationException();
                }

                Console.WriteLine(
                string.Format(
                    "OnNextAsync {0}: {1}",
                    value,
                    DateTime.Now));
            }

            public override Task OnNextAsync(
                T value, 
                CancellationToken cancellationToken)
            {
                if (Equals(value, 3))
                {
                    throw new InvalidOperationException();
                }

                return base.OnNextAsync(value, cancellationToken);
            }
        }
    }
}

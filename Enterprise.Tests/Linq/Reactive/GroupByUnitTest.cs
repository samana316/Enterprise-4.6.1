using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class GroupByUnitTest
    {
        private readonly School school = new School();

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupBySingleProperty()
        {
            try
            {
                Console.WriteLine("Group by a single property in an object:");

                var students = school.Students;
                var observable = school.ObservableStudents;

                var queryLastNames =
                    from student in observable
                    group student by student.LastName into newGroup
                    select newGroup;

                var count = await this.VerifyGroupingsAsync(queryLastNames);

                Assert.AreEqual(await students.CountAsync(), count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupBySingleExpression()
        {
            try
            {
                Console.WriteLine("Group by something other than a property of the object:");

                var students = school.Students;
                var observable = school.ObservableStudents;

                var queryFirstLetters =
                    from student in observable
                    group student by student.LastName[0];

                var count = await this.VerifyGroupingsAsync(queryFirstLetters);

                Assert.AreEqual(await students.CountAsync(), count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupByNumericRange()
        {
            try
            {
                Console.WriteLine("Group by numeric range and project into a new anonymous type:");

                var students = school.Students;
                var observable = school.ObservableStudents;

                var queryNumericRange =
                    from student in observable
                    let percentile = GetPercentile(student)
                    group new { student.FirstName, student.LastName } by percentile into percentGroup
                    select percentGroup;

                var count = await this.VerifyGroupingsAsync(queryNumericRange);

                Assert.AreEqual(await students.CountAsync(), count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupByAggregate()
        {
            try
            {
                Console.WriteLine("Group by a Boolean into two groups with string keys:");

                var students = school.Students;
                var observable = school.ObservableStudents;

                var queryGroupByAverages =
                    from student in observable
                    group new { student.FirstName, student.LastName }
                        by student.ExamScores.Average() > 75 into studentGroup
                    select studentGroup;

                var count = await this.VerifyGroupingsAsync(queryGroupByAverages);

                Assert.AreEqual(await students.CountAsync(), count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupByCompoundKey()
        {
            try
            {
                Console.WriteLine("Group and order by a compound key:");

                var students = school.Students;
                var observable = school.ObservableStudents;

                var queryHighScoreGroups =
                   from student in observable
                   group student by new
                   {
                       FirstLetter = student.LastName[0],
                       Score = student.ExamScores.FirstOrDefault() > 85
                   } into studentGroup
                   select studentGroup;

                var count = await this.VerifyGroupingsAsync(queryHighScoreGroups);

                Assert.AreEqual(await students.CountAsync(), count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task GroupByNestedQuery()
        {
            try
            {
                Console.WriteLine("Create a Nested Group:");

                var students = school.Students;
                var observable = school.ObservableStudents;

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

        private static int GetPercentile(
           IStudent s)
        {
            double avg = s.ExamScores.Average();
            return avg > 0 ? (int)avg / 10 : 0;
        }

        private Task<int> VerifyGroupingsAsync<TKey, TElement>(
           IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> query)
        {
            return this.VerifyGroupingsAsync(query, CancellationToken.None);
        }

        private async Task<int> VerifyGroupingsAsync<TKey, TElement>(
            IAsyncObservable<IAsyncGroupedObservable<TKey, TElement>> query,
            CancellationToken cancellationToken)
        {
            var handler = new AsyncGroupingHandler<TKey, TElement>();

            await query.ForEachAsync(handler.RunOuterAsync, cancellationToken);

            return handler.Count;
        }

        private sealed class AsyncGroupingHandler<TKey, TElement>
        {
            private int count;

            public int Count { get { return this.count; } }

            public async Task RunInnerAsync(
                TElement item,
                int index,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Console.Out.WriteLineAsync("Item: " + item);
                this.count++;
            }

            public async Task RunOuterAsync(
                IAsyncGroupedObservable<TKey, TElement> group,
                int index,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Console.Out.WriteLineAsync("Key: " + group.Key);

                await group.ForEachAsync(this.RunInnerAsync, cancellationToken);
            }
        }
    }
}

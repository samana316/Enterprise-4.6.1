using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Collections.Extensions;
using Enterprise.Core.Linq;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class GroupByUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupBySingleProperty()
        {
            try
            {
                Trace.WriteLine("Group by a single property in an object:");
                var students = new School().Students;

                var queryLastNames =
                    from student in students
                    group student by student.LastName into newGroup
                    orderby newGroup.Key
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
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupBySingleExpression()
        {
            try
            {
                Trace.WriteLine("Group by something other than a property of the object:");
                var students = new School().Students;

                var queryFirstLetters =
                    from student in students
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
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupByNumericRange()
        {
            try
            {
                Trace.WriteLine("Group by numeric range and project into a new anonymous type:");
                var students = new School().Students;

                var queryNumericRange =
                    from student in students
                    let percentile = GetPercentile(student)
                    group new { student.FirstName, student.LastName } by percentile into percentGroup
                    orderby percentGroup.Key
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
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupByAggregate()
        {
            try
            {
                Trace.WriteLine("Group by a Boolean into two groups with string keys:");
                var students = new School().Students;

                var queryGroupByAverages = 
                    from student in students
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
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupByCompoundKey()
        {
            try
            {
                Trace.WriteLine("Group and order by a compound key:");
                var students = new School().Students;

                var queryHighScoreGroups =
                   from student in students
                   group student by new
                   {
                       FirstLetter = student.LastName[0],
                       Score = student.ExamScores.FirstOrDefault() > 85
                   } into studentGroup
                   orderby studentGroup.Key.FirstLetter
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
        [TestCategory("Linq")]
        [TestCategory("GroupBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task GroupByNestedQuery()
        {
            try
            {
                Trace.WriteLine("Create a Nested Group:");
                var students = new School().Students;

                var queryNestedGroups =
                    from student in students
                    group student by student.Year into newGroup1
                    from newGroup2 in
                        (from student in newGroup1
                         group student by student.LastName)
                    group newGroup2 by newGroup1.Key;   

                foreach (var outerGroup in queryNestedGroups)
                {
                    Console.WriteLine("DataClass.Student Level = {0}", outerGroup.Key);
                    foreach (var innerGroup in outerGroup)
                    {
                        Console.WriteLine("\tNames that begin with: {0}", innerGroup.Key);
                        foreach (var innerGroupElement in innerGroup)
                        {
                            Console.WriteLine("\t\t{0} {1}", innerGroupElement.LastName, innerGroupElement.FirstName);
                        }
                    }
                }

                await Task.Yield();
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

        private async Task<int> VerifyGroupingsAsync<TKey, TElement>(
            IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> query)
        {
            var count = 0;

            using (var groupIterator = query.GetAsyncEnumerator())
            {
                while (await groupIterator.MoveNextAsync())
                {
                    var group = groupIterator.Current;

                    Trace.WriteLine(group.Key, "Key");

                    using (var iterator = group.GetAsyncEnumerator())
                    {
                        while (await iterator.MoveNextAsync())
                        {
                            Trace.WriteLine(iterator.Current, "Item");
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}

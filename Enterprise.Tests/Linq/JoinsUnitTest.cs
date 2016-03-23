using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class JoinsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("GroupJoin")]
        [TestCategory("Unit")]
        public async Task GroupJoin()
        {
            try
            {
                var magnus = new { FirstName = "Magnus", LastName = "Hedlund" };
                var terry = new { FirstName = "Terry", LastName = "Adams" };
                var charlotte = new { FirstName = "Charlotte", LastName = "Weiss" };
                var arlene = new { FirstName = "Arlene", LastName = "Huff" };

                var barley = new { Name = "Barley", Owner = terry };
                var boots = new { Name = "Boots", Owner = terry };
                var whiskers = new { Name = "Whiskers", Owner = charlotte };
                var bluemoon = new { Name = "Blue Moon", Owner = terry };
                var daisy = new { Name = "Daisy", Owner = magnus };

                var people = (new[] { magnus, terry, charlotte, arlene }).AsAsyncEnumerable();
                var pets = (new[] { barley, boots, whiskers, bluemoon, daisy }).AsAsyncEnumerable();

                var query = from person in people
                            join pet in pets on person equals pet.Owner into gj
                            from subpet in gj.DefaultIfEmpty()
                            select new
                            {
                                person.FirstName,
                                PetName = (subpet == null ? string.Empty : subpet.Name)
                            };

                await query.ForEachAsync(item => Trace.WriteLine(item, "Item"));

                var countExpected = 6;
                
                Assert.AreEqual(countExpected, await query.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Join")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task Join()
        {
            try
            {
                var outer = new School().Students;

                var inner = Enum.GetValues(typeof(GradeLevel))
                    .OfType<GradeLevel>()
                    .Select(value => new { Year = value, Description = value.ToString() });

                var query =
                    from student in outer
                    join grade in inner
                        on student.Year equals grade.Year
                    orderby grade.Year, student.LastName, student.FirstName
                    select new { Year = grade.Description, StudentName = student.ToString() };

                await query.ForEachAsync(item => Trace.WriteLine(item, "Item"));

                var countExpected = 12;

                Assert.AreEqual(countExpected, await query.CountAsync());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

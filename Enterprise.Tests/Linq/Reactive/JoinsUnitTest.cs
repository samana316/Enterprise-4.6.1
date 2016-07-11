using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class JoinsUnitTest
    {
        private readonly School school = new School();

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Join")]
        [TestCategory("Unit")]
        //[Timeout(5000)]
        public async Task JoinSimple()
        {
            try
            {
                var left = school.ObservableStudents;
                Trace.WriteLine(left, "Left");

                var right = Enum.GetValues(typeof(GradeLevel))
                   .OfType<GradeLevel>()
                   //.Where(x => x == GradeLevel.FirstYear)
                   .Select(value => new
                       {
                           Year = AsyncObservable.Return(value),
                           Description = value.ToString()
                       })
                   .ToAsyncObservable();
                Trace.WriteLine(right, "Right");

                var query =
                   from student in left
                   join grade in right
                       on AsyncObservable.Return(student.Year) equals grade.Year
                   //select new { Year = grade.Description, StudentName = student.ToString() };
                   select new
                   {
                       Element = student.ToString(),
                       KeyLeft = student.Year.ToString(),
                       KeyRight = grade.Description
                   };
                Trace.WriteLine(query, "Query");

                var countExpected = 12;
                var observer = new TestAsyncObserver<object>();

                using (await query.SubscribeAsync(observer)) { };

                Assert.AreEqual(countExpected, observer.Items.Count());
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

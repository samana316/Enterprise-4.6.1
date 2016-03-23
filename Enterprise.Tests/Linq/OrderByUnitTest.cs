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
    public sealed class OrderByUnitTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("OrderBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task OrderBySimple()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));
                var cancellationToken = cancelSource.Token;

                var source = (new[] { 9, 0, 2, 1, 0 }).AsAsyncEnumerable();

                var result =
                    from item in source
                    orderby item
                    select item;

                await result.ForEachAsync(item => 
                    Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var expected = new[] { 0, 0, 1, 2, 9 };
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
        [TestCategory("OrderBy")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task OrderByComplex()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));
                var cancellationToken = cancelSource.Token;

                var students = new School().Students;

                var result =
                    from student in students
                    where student.Year == GradeLevel.FirstYear
                    orderby student.LastName, student.FirstName
                    select student;

                await result.ForEachAsync(item =>
                {
                    Trace.WriteLine(DateTime.Now, "Timestamp");
                    Trace.WriteLine(item, "MoveNextAsync");
                }, cancellationToken);

                Assert.IsTrue(
                    await result.AllAsync(student => student.Year == GradeLevel.FirstYear));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Reverse")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ReverseSimple()
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                cancelSource.CancelAfter(TimeSpan.FromMilliseconds(1000));
                var cancellationToken = cancelSource.Token;

                var source = AsyncEnumerable.Range(1, 5);

                var result = source.Reverse();

                await result.ForEachAsync(item =>
                    Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var expected = Enumerable.Range(1, 5).Reverse();
                Assert.IsTrue(await result.SequenceEqualAsync(expected, cancellationToken));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Providers
{
    [TestClass]
    public sealed class AsyncEnumerableQueryTest
    {
        [TestMethod]
        [TestCategory("Linq.Providers")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task AsyncMethods()
        {
            try
            {
                var source = AsyncEnumerable.Range(1, 3).AsQueryable();
                Trace.WriteLine(source.ToString(), "Query");

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                await source.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await source.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(3, countActual);

                var sum = await source.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await source.AverageAsync(cancellationToken);
                Trace.WriteLine(average, "AverageAsync");

                Assert.AreEqual(sum / countActual, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Providers")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task PagingMethods()
        {
            try
            {
                var query = AsyncEnumerable.Range(1, 10).AsQueryable().Take(() => 3);

                Trace.WriteLine(query.ToString(), "Query");

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                await query.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await query.CountAsync(x => x > 0, cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(3, countActual);

                var sum = await query.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await query.AverageAsync(cancellationToken);
                Trace.WriteLine(average, "AverageAsync");

                Assert.AreEqual(sum / countActual, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Providers")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task ExpressionTest()
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                var cancellationToken = cts.Token;

                var source = AsyncEnumerable.Range(1, 10).Take(5);

                var expression = Expression.Constant(new AsyncEnumerableQuery<int>(source));
                Trace.WriteLine(expression.Type, "Expression");
                var query = new AsyncEnumerableQuery<int>(expression).AsQueryable();

                query =
                    from item in query
                    where item % 2 != 0
                    select item;

                Trace.WriteLine(query.ToString(), "Query");

                await query.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await query.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(3, countActual);

                var sum = await query.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await query.AverageAsync(cancellationToken);
                Trace.WriteLine(average, "AverageAsync");

                Assert.AreEqual(sum / countActual, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Providers")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task CreateQueryTest()
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                var cancellationToken = cts.Token;

                var source = AsyncEnumerable.Range(1, 10).Take(3);

                var expression = Expression.Constant(new AsyncEnumerableQuery<int>(source));
                Trace.WriteLine(expression.Type, "Expression");
                var query = new AsyncEnumerableQuery<int>(expression).AsQueryable();

                query = query.Provider.CreateQuery(expression).Cast<int>();

                Trace.WriteLine(query.ToString(), "Query");

                await query.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await query.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(3, countActual);

                var sum = await query.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await query.AverageAsync(cancellationToken);
                Trace.WriteLine(average, "AverageAsync");

                Assert.AreEqual(sum / countActual, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

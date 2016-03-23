using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enterprise.Tests.Linq.Helpers;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class IntegrationTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("WhereSelect")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task IOWhereSelect()
        {
            try
            {
                const int countExpected = 2;

                var source =
                    from item in DataReaderAdapter.Create(
                            this.GetConnectionString(),
                            "SELECT 1 UNION SELECT 2 UNION SELECT 3",
                            r => r.GetInt32(0))
                    where item % 2 != 0
                    select Convert.ToDecimal(item);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;

                await source.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var countActual = await source.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var expected = new [] { 1m, 3m };
                Assert.IsTrue(await source.SequenceEqualAsync(expected, cancellationToken));

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
        [TestCategory("Linq")]
        [TestCategory("SelectMany")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task IOSelectMany()
        {
            try
            {
                var source = AsyncEnumerable.Create<IAsyncEnumerable<int>>(async (y, ct) =>
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var reader = DataReaderAdapter.Create(
                            this.GetConnectionString(),
                            "SELECT 1 UNION SELECT 2 UNION SELECT 3",
                            r => r.GetInt32(0));

                        await y.ReturnAsync(reader, ct);
                    }
                });

                await source.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                var result =
                    from item in source
                    from subItem in item
                    where subItem % 2 != 0
                    orderby subItem descending
                    select Convert.ToDecimal(subItem);

                Assert.IsNotNull(result);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                var cancellationToken = cts.Token;

                await result.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                const int countExpected = 6;
                var countActual = await result.CountAsync(cancellationToken);
                Trace.WriteLine(countActual, "CountAsync");
                Assert.AreEqual(countExpected, countActual);

                var sum = await result.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await result.AverageAsync(cancellationToken);
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
        [TestCategory("Linq")]
        [TestCategory("Concat")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task IOConcat()
        {
            try
            {
                var first = DataReaderAdapter.Create(
                    this.GetConnectionString(),
                    "SELECT 1 UNION SELECT 2 UNION SELECT 3",
                    r => r.GetInt32(0));

                var second = DataReaderAdapter.Create(
                    this.GetConnectionString(),
                    "SELECT 4 UNION SELECT 5 UNION SELECT 6",
                    r => r.GetInt32(0));

                var third = AsyncEnumerable.Range(7, 4);

                var result =
                    from item in first.Concat(second).Concat(third)
                    where item % 2 != 0
                    select Convert.ToDecimal(item) + 0.5m;

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);

                var cancellationToken = cts.Token;
                await result.ForEachAsync(
                    item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                var expected = new[] { 1.5m, 3.5m, 5.5m, 7.5m, 9.5m };

                Assert.IsTrue(await result.SequenceEqualAsync(expected, cancellationToken));

                var count = await result.CountAsync(cancellationToken);
                Trace.WriteLine(count, "CountAsync");
                Assert.AreEqual(expected.Count(), count);

                var sum = await result.SumAsync(cancellationToken);
                Trace.WriteLine(sum, "SumAsync");

                var average = await result.AverageAsync(cancellationToken);
                Trace.Write(average, "AverageAsync");
                Assert.AreEqual(sum / count, average);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private ConnectionStringSettings GetConnectionString()
        {
            const string providerName = "System.Data.SqlClient";
            var provider = DbProviderFactories.GetFactory(providerName);

            var connectionStringBuilder = provider.CreateConnectionStringBuilder();
            connectionStringBuilder.Add("Data Source", @"(LocalDB)\v11.0");
            connectionStringBuilder.Add("Integrated Security", true);

            return new ConnectionStringSettings
            {
                ProviderName = providerName,
                ConnectionString = connectionStringBuilder.ConnectionString
            };
        }
    }
}

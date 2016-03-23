using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enterprise.Tests.Linq.Helpers;

namespace Enterprise.Tests.Linq.Providers
{
    [TestClass]
    public sealed class IntegrationTest
    {
        [TestMethod]
        [TestCategory("Linq.Providers")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task IOQuery()
        {
            try
            {
                using (var cts = new CancellationTokenSource(10000))
                {
                    var cancellationToken = cts.Token;

                    var first = DataReaderAdapter.Create(
                    this.GetConnectionString(),
                    "SELECT 1 UNION SELECT 2 UNION SELECT 3",
                    r => r.GetInt32(0)).AsQueryable();

                    var second = DataReaderAdapter.Create(
                        this.GetConnectionString(),
                        "SELECT 4 UNION SELECT 5 UNION SELECT 6",
                        r => r.GetInt32(0));

                    var third = 
                        from item in AsyncEnumerable.Range(7, 4)
                        orderby item
                        select item;

                    var result =
                       from item in first.Concat(second).Concat(third)
                       where item % 2 != 0
                       select Convert.ToDecimal(item) + 0.5m;

                    Trace.WriteLine(result.ToString());
                    await result.ForEachAsync(
                        item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                    var expected = new[] { 1.5m, 3.5m, 5.5m, 7.5m, 9.5m };

                    Assert.IsTrue(result.SequenceEqual(expected));

                    var count = await result.CountAsync(cancellationToken);
                    Trace.WriteLine(count, "CountAsync");
                    Assert.AreEqual(expected.Count(), count);

                    var sum = await result.SumAsync(cancellationToken);
                    Trace.WriteLine(sum, "SumAsync");

                    var average = await result.AverageAsync(cancellationToken);
                    Trace.Write(average, "AverageAsync");
                    Assert.AreEqual(sum / count, average);
                }
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

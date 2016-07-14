using System;
using System.Configuration;
using System.Data.Common;
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
    public sealed class IntegrationTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive.IO")]
        [TestCategory("ForEachAsync")]
        [TestCategory("Integration")]
        [Timeout(5000)]
        public async Task IOForEachAsync()
        {
            try
            {
                var source = this.GetSampleSource();

                await source.ForEachAsync(x => Trace.WriteLine(x, "OnNext"));

                var observer1 = new TestAsyncObserver<int>();
                using (await source.SubscribeAsync(observer1)) { }
                Assert.IsTrue(await observer1.Items.SequenceEqualAsync(new[] { 1, 2, 3 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.IO")]
        [TestCategory("SelectMany")]
        [TestCategory("Integration")]
        [Timeout(5000)]
        public async Task IOSelectMany()
        {
            try
            {
                var source = this.GetSampleSource();
                var query1 = AsyncObservable.Repeat<IAsyncObservable<int>>(source, 3);

                var query2 =
                    from item in query1
                    from subItem in item
                    where subItem % 2 != 0
                    select Convert.ToDecimal(subItem);

                var observer1 = new TestAsyncObserver<decimal>();

                using (await query2.SubscribeAsync(observer1)) { }

                Assert.IsTrue(
                    await observer1.Items.SequenceEqualAsync(new decimal[] { 1, 3, 1, 3, 1, 3 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive.IO")]
        [TestCategory("WhereSelect")]
        [TestCategory("Integration")]
        [Timeout(5000)]
        public async Task IOWhereSelect()
        {
            try
            {
                var source = this.GetSampleSource();

                var query =
                    from item in source
                    where item % 2 != 0
                    select Convert.ToDecimal(item);

                var observer = new TestAsyncObserver<decimal>();

                using (await query.SubscribeAsync(observer)) { }

                Assert.IsTrue(
                    await observer.Items.SequenceEqualAsync(new decimal[] { 1, 3 }));
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

        private IAsyncObservable<int> GetSampleSource()
        {
            return new DataReaderObservable<int>(
                this.GetConnectionString(),
                "SELECT 1 UNION SELECT 2 UNION SELECT 3",
                r => r.GetInt32(0));
        }
    }
}

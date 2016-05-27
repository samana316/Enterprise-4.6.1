using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Tests.Linq.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class AggregateAsyncTest
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Aggregate")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task AggregateAsync()
        {
            try
            {
                var source = StreamAdapter.Create(this.CreateMemoryStream);

                await source.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"));

                var result = await source.AggregateAsync((x, y) => x + y);
                Trace.WriteLine(result, "AggregateAsync");

                Assert.AreEqual("ABCDEFG", result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Aggregate")]
        [TestCategory("Integration")]
        [Timeout(30000)]
        public async Task IOAggregateAsync()
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(5000);

                    var cancellationToken = cts.Token;
                    var source = StreamAdapter.Create(this.CreateFileStream);

                    await source.ForEachAsync(item => Trace.WriteLine(item, "MoveNextAsync"), cancellationToken);

                    var result = await source.AggregateAsync((x, y) => x + y, cancellationToken);
                    Trace.WriteLine(result, "AggregateAsync");

                    Assert.AreEqual("ABCDEFG", result);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private Stream CreateMemoryStream()
        {
            const string text = 
@"A
B
C
D
E
F
G";

            var buffer = Encoding.Default.GetBytes(text);

            var stream = new MemoryStream(buffer);

            return stream;
        }

        private Stream CreateFileStream()
        {
            return File.OpenRead(@"C:\Temp\Temp1.txt");
        }
    }
}

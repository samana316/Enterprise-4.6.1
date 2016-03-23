using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.IO;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.IO
{
    [TestClass]
    public sealed class AsyncFileTest
    {
        [TestMethod]
        [TestCategory("IO")]
        [Timeout(10000)]
        public async Task AsyncFileSimple()
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);
                var cancellationToken = cts.Token;

                const string path1 = @"C:\Temp\Temp1.txt";
                const string path2 = @"C:\Temp\Temp2.txt";
                var encoding = Encoding.UTF8;

                File.Delete(path2);

                await AsyncFile.CopyAsync(path1, path2, true, cancellationToken);

                var source = AsyncEnumerable.Create<string>(async y =>
                {
                    await y.ReturnAsync("X");
                    await y.ReturnAsync("Y");
                    await y.ReturnAsync("Z");
                });

                await AsyncFile.AppendAllLinesAsync(
                    path2,
                    source,
                    encoding,
                    cancellationToken);

                var buffer = await AsyncFile.ReadAllBytesAsync(path2, cancellationToken);

                Trace.WriteLine(encoding.GetString(buffer), "ReadAllBytesAsync");

                var lines = AsyncFile.ReadLines(path2, encoding);

                await lines.ForEachAsync(line =>
                {
                    Trace.WriteLine(line, "ReadLines");
                });

                await Task.Delay(10);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("IO")]
        [Timeout(10000)]
        public async Task AsyncFileSimpleLinq()
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);
                var cancellationToken = cts.Token;

                const string path1 = @"C:\Temp\Temp1.txt";
                const string path2 = @"C:\Temp\Temp2.txt";
                var encoding = Encoding.UTF8;

                File.Delete(path2);

                var contents = AsyncFile.ReadLines(path1, encoding);
                var nums = AsyncEnumerable.Range(1, 7);
                var query = contents.Zip(nums, (c, num) => c + num);

                await AsyncFile.WriteAllLinesAsync(path2, query, encoding, cancellationToken);

                var result = await AsyncFile.ReadAllTextAsync(path2, encoding, cancellationToken);
                Trace.WriteLine(result);

                await Task.Delay(10);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

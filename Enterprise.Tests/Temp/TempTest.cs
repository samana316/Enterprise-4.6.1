using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.IO;
using Enterprise.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class TempTest
    {
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public void TempAsyncTest()
        {
            try
            {
                var query = new EnumerableQuery<int>(Enumerable.Range(1, 5));

                var sum = query.Sum();


            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

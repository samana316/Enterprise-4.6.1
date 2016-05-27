using System;
using System.Collections.Generic;
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
                var query = TestIterator();

                Trace.WriteLine(query);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private IEnumerable<int> TestIterator()
        {
            yield return 1;
        }
    }
}

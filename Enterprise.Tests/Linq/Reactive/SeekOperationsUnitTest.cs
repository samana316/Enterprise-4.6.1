using System;
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
    public sealed class SeekOperationsUnitTest
    {
        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("SeekOperations")]
        [TestCategory("Unit")]
        //[Timeout(5000)]
        public async Task ElementAtSimple()
        {
            try
            {
                var observable = AsyncObservable.Range(1, 3);

                var elementAt1 = observable.ElementAt(1);
                var elementAt4 = observable.ElementAt(4);

                var concat = elementAt1.Concat(elementAt4);
                var testObserver = new TestAsyncObserver<int>();
                
                using (await concat.SubscribeRawAsync(testObserver)) { };

                Assert.AreEqual(2, await testObserver.Items.SingleAsync());
                Assert.IsTrue(await testObserver.Errors.SingleAsync() is IndexOutOfRangeException);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}

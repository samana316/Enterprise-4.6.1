using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class DelegateTest
    {
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public void TempDelegateTest1()
        {
            try
            {
                Func<object, bool> predicate1 = Overload1;
                Func<object, int, bool> predicate2 = CreateOverload2(predicate1);

                var result = predicate2(null, 0);
                Trace.WriteLine(result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.IsTrue(exception is NotImplementedException);
            }
        }

        private Func<TSource, int, TResult> CreateOverload2<TSource, TResult>(
            Func<TSource, TResult> func)
        {
            var builder = new OverloadBuilder<TSource, TResult>(func);

            return builder.Overload;
        }

        private static bool Overload1<TSource>(
            TSource value)
        {
            throw new NotImplementedException();
        }

        private sealed class OverloadBuilder<TSource, TResult>
        {
            private readonly Func<TSource, TResult> func;

            public OverloadBuilder(
                Func<TSource, TResult> func)
            {
                this.func = func;
            }

            public TResult Overload(
                TSource value,
                int index)
            {
                return this.func(value);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class CustomAwaitableTest
    {
        [TestMethod]
        [TestCategory("Awaitable")]
        [Timeout(60000)]
        public async Task CombineAwaitables()
        {
            try
            {
                for (var i = 1; i <= 5; i++)
                {
                    var value = await this.GetValueAsync<int>();
                    value += i;

                    await Task.Run(() => Trace.WriteLine(value));
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private IAwaitable<T> GetValueAsync<T>()
        {
            return new TestAwaitable<T>();
        }

        private class TestAwaitable<T> : IAwaitable<T>
        {
            public IAwaiter<T> GetAwaiter()
            {
                return new TestAwaiter();
            }

            IAwaiter IAwaitable.GetAwaiter()
            {
                return this.GetAwaiter();
            }

            private class TestAwaiter : IAwaiter<T>
            {
                public bool IsCompleted { get; set; }

                public T GetResult()
                {
                    this.InternalComplete();

                    return default(T);
                }

                public void OnCompleted(
                    Action continuation)
                {
                    continuation();
                }

                void IAwaiter.GetResult()
                {
                    this.InternalComplete();
                }

                private void InternalComplete()
                {
                    this.IsCompleted = true;
                }
            }
        }
    }
}

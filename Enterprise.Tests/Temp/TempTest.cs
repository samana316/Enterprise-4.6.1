using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Providers;
using Enterprise.Core.Linq.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class TempTest
    {
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempAsyncTest()
        {
            try
            {
                var iterator = AsyncEnumerable.Range(1, 1); //TestIterator().ToArray().AsAsyncEnumerable();

                Trace.WriteLine(iterator, "IAsyncEnumerable1");

                var query = iterator.AsQueryable();

                Trace.WriteLine(query, "IQueryable1");

                var result =
                    from element in query
                    join item in iterator on element equals item
                    where element > 1
                    orderby item
                    select element;

                Trace.WriteLine(result, "IQueryable2");

                var result2 =
                    from item in iterator
                    join element in query on item equals element
                    where item > 1
                    orderby element
                    select item;

                Trace.WriteLine(result2, "IAsyncEnumerable2");

                var result3 = result2.AsQueryable();
                Trace.WriteLine(result3, "IQueryable3");

                await Task.Yield();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempAsyncTest2()
        {
            var source = new CancellationTokenSource();

            try
            {
                var cancellationToken = source.Token;
                
                var num = 0L;

                Func<CancellationToken, Task> funcAsync = async (ct) =>
                {
                    try
                    {
                        while (true)
                        {
                            ct.ThrowIfCancellationRequested();

                            await Task.Delay(10, ct);

                            num++;

                            //await Task.Yield();
                        }
                    }
                    catch (Exception exception)
                    {
                        Trace.WriteLine(exception);
                    }
                };

                var task = funcAsync(cancellationToken);

                Trace.WriteLine(task.Status);
                await Task.Delay(100);

                Trace.WriteLine(task.Status);

                try
                {
                    source.Cancel();
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception);
                }

                Trace.WriteLine(task.Status);
                try
                {
                    task.Dispose();
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception);
                }

                var sample1 = num;
                Trace.WriteLine(sample1);
                await Task.Delay(10);

                var sample2 = num;
                Trace.WriteLine(sample2);
                Assert.AreEqual(sample1, sample2);

                await task;

                await Task.Yield();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                source.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(5000)]
        public async Task TempAsyncTest3()
        {
            try
            {
                IDisposable obj = null;

                using (var source = new CancellationTokenSource(1000))
                {
                    try
                    {
                        obj = await this.TempMethodAsync(source.Token);
                    }
                    finally
                    {
                        Trace.WriteLine(obj.GetType().AssemblyQualifiedName);
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(5000)]
        public async Task TempAsyncTest4()
        {
            try
            {
                var random = new Random();
                Func<int, Task> funcAsync = async (x) =>
                {
                    await Task.Yield();
                    await Task.Delay(200);
                    await Console.Out.WriteLineAsync(x.ToString());
                };

                var source = Enumerable.Range(1, 9);
                var tasks =
                    from item in source
                    select funcAsync(item);

                await Task.WhenAll(tasks);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private async Task<IDisposable> TempMethodAsync(
            CancellationToken cancellationToken)
        {
            var task = Console.Out.WriteLineAsync("OnNext");

            for (var i = 0; true; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await task;
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempParallelAsyncTest()
        {
            try
            {
                var query =
                    from item in AsyncEnumerable.Range(0, 10)
                    let timestamp = DateTime.Now
                    let text = item.ToString() + ": " + timestamp.Ticks
                    select new Action(async () => { await Task.Delay(10); await Console.Out.WriteLineAsync(text); });

                await Task.Run(async () => Parallel.Invoke(await query.ToArrayAsync()));

                await Task.Delay(10);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempParallelAsyncTest2()
        {
            try
            {
                var query =
                    from item in AsyncEnumerable.Range(0, 10)
                    let timestamp = DateTime.Now
                    let text = item.ToString() + ": " + timestamp.Ticks
                    select Task.Run(
                        new Func<Task>(async () => 
                        {
                            await Task.Delay(10);
                            await Console.Out.WriteLineAsync(text);
                        }));

                var tasks = await query.ToArrayAsync();

                await Task.WhenAll(tasks);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempReflectionAsyncTest()
        {
            try
            {
                var method = this.GetType().GetMethod(
                    "WriteLineAsync", BindingFlags.Instance | BindingFlags.NonPublic);

                var task = method.Invoke(this, new object[] { "Test1234" }) as Task;

                await task;
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempExpressionAsyncTest()
        {
            try
            {
                Expression<Func<Task<int>>> expression = () => Task.FromResult(1);

                var funcAsync = expression.Compile();

                var result = await funcAsync();

                Trace.WriteLine(result);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempDebuggerTest()
        {
            try
            {
                Trace.Write(long.MaxValue);
                //bool isDebuggerPresent = false;
                //CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);

                //var isDebuggerAttached = Debugger.IsAttached;

                //Trace.WriteLine(isDebuggerPresent);
                //Trace.WriteLine(isDebuggerAttached);

                //await Task.Yield();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempTryCatchAsyncTest()
        {
            try
            {
                await this.WriteLineAsync("Try block...");
                await TaskHelpers.ThrowAsync<Exception>(new InvalidOperationException("Throw here..."));
            }
            catch (Exception exception)
            {
                await this.WriteLineAsync(exception.ToString());
            }
        }

        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempAsyncIOTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var task = this.TempIOMethodAsync(cancellationToken);
                task.Dispose();
                Trace.WriteLine("await task");

                await task.ContinueWith(async t => Trace.WriteLine(await t));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        private IEnumerable<int> TestIterator()
        {
            yield return 1;
        }

        private async Task WriteLineAsync(
            string x)
        {
            await Task.Delay(100);
            await Console.Out.WriteLineAsync(x);
        }

        private async Task<int> TempIOMethodAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var provider = DbProviderFactories.GetFactory("System.Data.SqlClient");
            Trace.WriteLine("GetFactory");

            using (var connection = provider.CreateConnection())
            {
                connection.ConnectionString = @"Data Source=dev-jxfw-cls110.fanatics.corp;Initial Catalog=eWarehouse;Integrated Security=True;";
                Trace.WriteLine("CreateConnection");

                await connection.OpenAsync(cancellationToken);
                Trace.WriteLine("OpenAsync");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    Trace.WriteLine("CreateCommand");

                    var result = await command.ExecuteScalarAsync(cancellationToken);
                    Trace.WriteLine("ExecuteAsync");

                    return Convert.ToInt32(result);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
    }
}

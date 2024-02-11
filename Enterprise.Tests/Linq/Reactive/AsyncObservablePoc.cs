using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Linq.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class AsyncObservablePoc
    {
        [TestMethod]
        [TestCategory("Temp")]
        [Timeout(10000)]
        public async Task TempAsyncObservableTest()
        {
            var source = new CancellationTokenSource();

            try
            {
                var cancellationToken = source.Token;

                var observable = new TestAsyncObservableImpl();
                var observer = new TestAsyncObserverImpl();

                using (var subscription = await observable.SubscribeAsync(observer, cancellationToken))
                {
                    Trace.WriteLine(subscription);
                }

                Trace.WriteLine(observable.Current, "Current");
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

        private sealed class TestAsyncObservableImpl : DisposableBase, IAsyncObservable<long>
        {
            public long Current { get; private set; }

            public IDisposable Subscribe(
                IObserver<long> observer)
            {
                throw new NotImplementedException();
            }

            public async Task<IDisposable> SubscribeAsync(
                IAsyncObserver<long> observer, 
                CancellationToken cancellationToken)
            {
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await Task.Delay(10, cancellationToken);

                        Current++;

                        await observer.OnNextAsync(Current, cancellationToken);
                    }
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                finally
                {
                    observer.OnCompleted();
                }

                return observer as IDisposable;
            }
        }

        private sealed class TestAsyncObserverImpl : DisposableBase, IAsyncObserver<long>
        {
            private bool completed;

            public void OnCompleted()
            {
                if (this.completed)
                {
                    return;
                }

                Trace.WriteLine("OnCompleted");

                this.Dispose();

                this.completed = true;
            }

            public void OnError(
                Exception error)
            {
                Trace.WriteLine(error, "OnError");

                this.OnCompleted();
            }

            public void OnNext(
                long value)
            {
                throw new InvalidOperationException();
            }

            public async Task OnNextAsync(
                long value, 
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (value >= 10)
                {
                    throw new InvalidOperationException("Break from OnNextAsync");
                }

                await Console.Out.WriteLineAsync(
                    string.Format(
                        "OnNextAsync {0}: {1}",
                        value,
                        DateTime.Now));
            }

            protected override void Dispose(
                bool disposing)
            {
                if (this.completed)
                {
                    return;
                }

                base.Dispose(disposing);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Collections.Extensions;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class FizzBuzzTest
    {
        const int DefaultStart = 1;

        const int DefaultCount = 45;

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public void FizzBuzzIterator()
        {
            try
            {
                var source = FizzBuzz.Iterator();
                var observer = new TestObserver<string>();

                source.ForEach(observer.OnNext);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public void FizzBuzzProjectionIterator()
        {
            try
            {
                var source = Enumerable.Range(DefaultStart, DefaultCount);
                var query = source.Select(FizzBuzz.Compute);

                var observer = new TestObserver<string>();

                query.ForEach(observer.OnNext);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public async Task FizzBuzzIteratorAsync()
        {
            try
            {
                var source = FizzBuzz.AsyncIterator();
                var observer = new TestAsyncObserver<string>();

                using (var enumerator = source.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        await observer.OnNextAsync(enumerator.Current);
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
        [TestCategory("Linq")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public async Task FizzBuzzProjectionIteratorAsync()
        {
            try
            {
                var source = AsyncEnumerable.Range(DefaultStart, DefaultCount);
                var query = source.Select(FizzBuzz.Compute);
                var observer = new TestAsyncObserver<string>();

                using (var enumerator = query.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        await observer.OnNextAsync(enumerator.Current);
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
        [TestCategory("Linq.Reactive")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public void FizzBuzzObservable()
        {
            try
            {
                var source = FizzBuzz.Observable();
                var observer = new TestObserver<string>();

                using (source.Subscribe(observer)) { }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public async Task FizzBuzzObservableAsync()
        {
            try
            {
                var source = FizzBuzz.AsyncObservable();
                var observer = new TestAsyncObserver<string>();

                using (await source.SubscribeAsync(observer)) { }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public async Task FizzBuzzProjectionObservableAsync()
        {
            try
            {
                var source = AsyncObservable.Range(DefaultStart, DefaultCount);
                var query = source.Select(FizzBuzz.Compute);
                var observer = new TestAsyncObserver<string>();

                using (await query.SubscribeAsync(observer)) { }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("FizzBuzz")]
        [TestCategory("Unit")]
        public async Task FizzBuzzObserverAsync()
        {
            try
            {
                var source = AsyncObservable.Range(DefaultStart, DefaultCount);
                var observer = FizzBuzz.AsyncObserver();

                using (await source.SubscribeAsync(observer)) { }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private static class FizzBuzz
        {
            public static IEnumerable<string> Iterator(
                int start = DefaultStart,
                int count = DefaultCount)
            {
                for (var i = start; i <= count; i++)
                {
                    yield return Compute(i);
                }

                yield break;
            }

            public static IAsyncEnumerable<string> AsyncIterator(
                int start = DefaultStart,
                int count = DefaultCount)
            {
                return AsyncEnumerable.Create<string>(async (yielder, cancellationToken) =>
                {
                    for (var i = start; i <= count; i++)
                    {
                        await yielder.ReturnAsync(Compute(i), cancellationToken);
                    }

                    await yielder.BreakAsync(cancellationToken);
                });
            }

            public static IObservable<string> Observable(
                int start = DefaultStart,
                int count = DefaultCount)
            {
                return new FizzBuzzAsyncObservable(start, count);
            }

            public static IAsyncObservable<string> AsyncObservable(
                int start = DefaultStart,
                int count = DefaultCount)
            {
                return new FizzBuzzAsyncObservable(start, count);
            }

            public static IObserver<int> Observer(
                IObserver<string> observer = null)
            {
                var asyncObserver = observer == null ?
                    new TestAsyncObserver<string>() : observer.AsAsyncObserver();

                return new FizzBuzzAsyncObserver(asyncObserver);
            }

            public static IAsyncObserver<int> AsyncObserver(
                IAsyncObserver<string> observer = null)
            {
                return new FizzBuzzAsyncObserver(observer ?? new TestAsyncObserver<string>());
            }

            public static string Compute(
                int value)
            {
                var flag = false;
                var builder = new StringBuilder();

                if (value % 3 == 0)
                {
                    flag = true;
                    builder.Append("Fizz");
                }
                if (value % 5 == 0)
                {
                    flag = true;
                    builder.Append("Buzz");
                }

                if (!flag)
                {
                    builder.Append(value);
                }

                return builder.ToString();
            }

            private class FizzBuzzAsyncObservable : IAsyncObservable<string>
            {
                private readonly int start;

                private readonly int count;

                public FizzBuzzAsyncObservable(
                    int start,
                    int count)
                {
                    this.start = start;
                    this.count = count;
                }

                public IDisposable Subscribe(
                    IObserver<string> observer)
                {
                    for (var i = start; i <= count; i++)
                    {
                        observer.OnNext(Compute(i));
                    }

                    observer.OnCompleted();

                    return TestSubscription.Instance;
                }

                public async Task<IDisposable> SubscribeAsync(
                    IAsyncObserver<string> observer,
                    CancellationToken cancellationToken)
                {
                    for (var i = start; i <= count; i++)
                    {
                        await observer.OnNextAsync(Compute(i), cancellationToken);
                    }

                    observer.OnCompleted();

                    return TestSubscription.Instance;
                }
            }

            private class FizzBuzzAsyncObserver : IAsyncObserver<int>
            {
                private readonly IAsyncObserver<string> observer;

                public FizzBuzzAsyncObserver(
                    IAsyncObserver<string> observer)
                {
                    this.observer = observer;
                }

                public void OnCompleted()
                {
                    this.observer.OnCompleted();
                }

                public void OnError(
                    Exception error)
                {
                    this.observer.OnError(error);
                }

                public void OnNext(
                    int value)
                {
                    this.OnNextAsync(value).Wait();
                }

                public Task OnNextAsync(
                    int value, 
                    CancellationToken cancellationToken)
                {
                    return this.observer.OnNextAsync(Compute(value), cancellationToken);
                }
            }
        }

        private class TestObserver<T> : TestAsyncObserver<T>
        {
            public override void OnNext(
                T value)
            {
                this.OnNextAsync(value).Wait();
            }
        }
    }
}

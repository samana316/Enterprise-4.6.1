using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Reactive;
using Enterprise.Tests.Linq.Reactive.Helpers;
using Enterprise.Tests.Linq.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq.Reactive
{
    [TestClass]
    public sealed class AggregateFunctionsUnitTest
    {
        private readonly School school = new School();

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Max")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task MaxSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(1);
                });
                Trace.WriteLine(observable, "observable");

                var max = observable.Max();
                Trace.WriteLine(max, "max");

                var testObserver = new TestAsyncObserver<long>();
                using (await max.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new long[] { 3 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Max")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task MaxNullable()
        {
            try
            {
                var observable = AsyncObservable.Create<long?>(async observer =>
                {
                    await observer.OnNextAsync(null);
                });
                Trace.WriteLine(observable, "observable");

                var max = observable.Max();
                Trace.WriteLine(max, "max");

                var testObserver = new TestAsyncObserver<long?>();
                using (await max.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new long?[] { null }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Max")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task MaxNullableProjection()
        {
            try
            {
                var testObserver = new TestAsyncObserver<double?>();
                var observable = school.ObservableStudents;
                Trace.WriteLine(observable, "observable");

                var max = observable.Max(new Func<IStudent, double?>(GetPercentile));
                Trace.WriteLine(max, "max");

                using (await max.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new double?[] { 9.225 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq.Reactive")]
        [TestCategory("Min")]
        [TestCategory("Unit")]
        [Timeout(5000)]
        public async Task MinSimple()
        {
            try
            {
                var observable = AsyncObservable.Create<long>(async observer =>
                {
                    await observer.OnNextAsync(1);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(3);
                    await observer.OnNextAsync(2);
                    await observer.OnNextAsync(1);
                });
                Trace.WriteLine(observable, "observable");

                var min = observable.Min();
                Trace.WriteLine(min, "min");

                var testObserver = new TestAsyncObserver<long>();
                using (await min.SubscribeAsync(testObserver)) { }

                Assert.IsTrue(
                    await testObserver.Items.SequenceEqualAsync(new long[] { 1 }));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private static double? GetPercentile(
            IStudent student)
        {
            double? percentile = student.ExamScores.Average() / 10d;

            return percentile.GetValueOrDefault() > 0 ? percentile : null;
        }
    }
}

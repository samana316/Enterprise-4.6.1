using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq.Reactive;

namespace Enterprise.Tests.Linq.TestDomain
{
    partial class School
    {
        public IAsyncObservable<IStudent> ObservableStudents
        {
            get { return this.GetObservableStudents(); }
        }

        private IAsyncObservable<IStudent> GetObservableStudents()
        {
            return new ObservableStudentsImpl();
        }

        private sealed class ObservableStudentsImpl : IAsyncObservable<IStudent>
        {
            public IDisposable Subscribe(
                IObserver<IStudent> observer)
            {
                throw new NotImplementedException();
            }

            public async Task<IDisposable> SubscribeAsync(
                IAsyncObserver<IStudent> observer, 
                CancellationToken cancellationToken)
            {
                foreach (var student in students)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await observer.OnNextAsync(student, cancellationToken);
                }

                return null;
            }
        }
    }
}

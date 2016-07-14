using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Linq.Reactive
{
    internal abstract partial class AsyncObservableBase<T>
    {
        public override bool Equals(
            object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            IAsyncObservable<bool> sequenceEqual = null;

            var observable = obj as IObservable<T>;
            if (!ReferenceEquals(observable, null))
            {
                sequenceEqual = this.SequenceEqual(observable);
            }

            var enumerable = obj as IEnumerable<T>;
            if (!ReferenceEquals(enumerable, null))
            {
                sequenceEqual = this.SequenceEqual(enumerable);
            }

            if (!ReferenceEquals(sequenceEqual, null))
            {
                return this.InternalEqualsAsync(sequenceEqual).Result;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<IAsyncObservable<T>>.Default.GetHashCode(this);
        }

        private async Task<bool> InternalEqualsAsync(
            IAsyncObservable<bool> sequenceEqual)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(1000);
                var cancellationToken = cancellationTokenSource.Token;

                var result = false;
                await sequenceEqual.ForEachAsync(value => result = value);

                return result;
            }
        }

        internal Task<IDisposable> SubscribeRawAsync(
           IAsyncObserver<T> observer,
           CancellationToken cancellationToken)
        {
            return this.SubscribeCoreAsync(observer, cancellationToken);
        }
    }
}

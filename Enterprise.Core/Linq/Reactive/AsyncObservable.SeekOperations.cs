using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Resources;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TSource> ElementAt<TSource>(
            this IAsyncObservable<TSource> source,
            int index)
        {
            Check.NotNull(source, "source");
            
            var list = source as IReadOnlyList<TSource>;
            if (list != null)
            {
                var element = list[index];
                return Return(element);
            }

            if (index < 0)
            {
                Error.ArgumentOutOfRange("index");
            }

            return new ElementAtAsyncObservable<TSource>(source, index, true);
        }

        private sealed class ElementAtAsyncObservable<TSource> : AsyncObservableBase<TSource>
        {
            private readonly IAsyncObservable<TSource> source;

            private int index;

            private readonly bool throwOnEmpty;

            public ElementAtAsyncObservable(
                IAsyncObservable<TSource> source, 
                int index, 
                bool throwOnEmpty)
            {
                this.source = source;
                this.index = index;
                this.throwOnEmpty = throwOnEmpty;
            }

            protected override async Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TSource> observer, 
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}

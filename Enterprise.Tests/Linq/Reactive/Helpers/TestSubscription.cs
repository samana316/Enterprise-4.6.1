using System;
using System.Diagnostics;
using Enterprise.Core.Common;

namespace Enterprise.Tests.Linq.Reactive.Helpers
{
    internal sealed class TestSubscription : DisposableBase
    {
        public static readonly IDisposable Instance = new TestSubscription();

        private TestSubscription()
        {
        }

        protected override void Dispose(
            bool disposing)
        {
            Trace.WriteLine("Dispose", "Subscription");

            base.Dispose(disposing);
        }
    }
}

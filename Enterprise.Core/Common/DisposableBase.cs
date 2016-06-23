using System;

namespace Enterprise.Core.Common
{
    [Serializable]
	public abstract class DisposableBase : IDisposable
	{
        private readonly object sink = new object();

		~DisposableBase()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
            lock (sink)
            {
                this.Dispose(true);
            }

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(
			bool disposing)
		{
		}
	}
}

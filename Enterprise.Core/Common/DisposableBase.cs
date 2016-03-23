using System;

namespace Enterprise.Core.Common
{
    [Serializable]
	public abstract class DisposableBase : IDisposable
	{
		~DisposableBase()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(
			bool disposing)
		{
		}
	}
}

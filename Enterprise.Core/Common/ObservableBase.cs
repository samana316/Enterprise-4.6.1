using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Core.Common
{
    [Serializable]
    [Obsolete]
	public abstract class ObservableBase<T> : DisposableBase, IObservable<T>
	{
		private readonly ICollection<IObserver<T>> observers = new List<IObserver<T>>();

		private readonly ICollection<IDisposable> subscriptions = new List<IDisposable>();

		private readonly object sink = new object();

		internal ICollection<IObserver<T>> Observers
		{
			get { return this.observers; }
		}

		public IDisposable Subscribe(
			IObserver<T> observer)
		{
			lock (sink)
			{
				if (!this.observers.Contains(observer))
				{
					this.observers.Add(observer);
				}

				var subscription = new Subscription(this, observer);

				this.subscriptions.Add(subscription);

				return subscription;
			}
		}

		protected override void Dispose(
			bool disposing)
		{
			lock (sink)
			{
				if (disposing)
				{
					foreach (var subscription in this.subscriptions)
					{
						if (ReferenceEquals(subscription, null))
						{
							continue;
						}

						subscription.Dispose();
					}

					this.observers.Clear();
				}

				base.Dispose(disposing);
			}
		}

		protected virtual void OnCompleted()
		{
			lock (sink)
			{
				foreach (var observer in this.observers.ToArray())
				{
					if (observer != null)
					{
						observer.OnCompleted();
					}
				}
			}
		}

		protected virtual void OnError(
			Exception error)
		{
			lock (sink)
			{
				foreach (var observer in this.observers)
				{
					if (observer != null)
					{
						observer.OnError(error);
					}
				}
			}
		}

		protected virtual void OnNext(
			T value)
		{
			lock (sink)
			{
				foreach (var observer in this.observers)
				{
					if (observer != null)
					{
						observer.OnNext(value);
					}
				}
			}
		}

        [Serializable]
		private class Subscription : DisposableBase
		{
			private readonly object sink = new object();

			private readonly ObservableBase<T> observable;

			private IObserver<T> observer;

			public Subscription(
				ObservableBase<T> observable,
				IObserver<T> observer)
			{
				this.observable = observable;
				this.observer = observer;
			}

			protected override sealed void Dispose(
				bool disposing)
			{
				lock (sink)
				{
					if (disposing)
					{
						if (this.observer != null && 
							this.observable.observers.Contains(this.observer))
						{
							this.observable.observers.Remove(this.observer);
						}
					}

					base.Dispose(disposing);
				}
			}
		}
	}
}

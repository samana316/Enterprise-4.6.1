using System;

namespace Enterprise.Core.Common
{
    public abstract class NotifyUnhandledException : INotifyUnhandledException
    {
        public virtual event UnhandledExceptionEventHandler UnhandledException;

        protected virtual void OnUnhandledException(
            Exception exception)
        {
            var handler = this.UnhandledException;

            if (handler != null)
            {
                handler(this, new UnhandledExceptionEventArgs(exception, false));
            }
        }
    }
}

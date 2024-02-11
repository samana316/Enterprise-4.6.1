using System;

namespace Enterprise.Core.Common
{
    public interface INotifyUnhandledException
    {
        event UnhandledExceptionEventHandler UnhandledException;
    }
}

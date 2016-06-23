using System;

namespace Enterprise.Core.Linq.Reactive
{
    internal interface IPartialObserver
    {
        void OnError(Exception error);

        void OnCompleted();
    }
}

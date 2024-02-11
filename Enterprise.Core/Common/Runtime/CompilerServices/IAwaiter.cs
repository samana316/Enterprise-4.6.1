using System.Runtime.CompilerServices;

namespace Enterprise.Core.Common.Runtime.CompilerServices
{
    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }

        void GetResult();
    }

    public interface IAwaiter<TResult> : IAwaiter
    {
        new TResult GetResult();
    }
}

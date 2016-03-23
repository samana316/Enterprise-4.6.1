namespace Enterprise.Core.Common.Runtime.CompilerServices
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<TResult> : IAwaitable
    {
        new IAwaiter<TResult> GetAwaiter();
    }
}

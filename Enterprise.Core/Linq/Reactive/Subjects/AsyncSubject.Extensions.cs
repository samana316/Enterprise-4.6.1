namespace Enterprise.Core.Linq.Reactive.Subjects
{
    public static class AsyncSubject
    {
        public static IAsyncSubject<T> Create<T>()
        {
            return new EmptyAsyncSubject<T>();
        } 

        private sealed class EmptyAsyncSubject<T> : AsyncRealSubject<T>
        {
        }
    }
}

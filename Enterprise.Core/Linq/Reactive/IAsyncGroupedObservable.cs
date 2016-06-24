namespace Enterprise.Core.Linq.Reactive
{
    public interface IAsyncGroupedObservable<out TKey, out TElement> : IAsyncObservable<TElement>
    {
        TKey Key { get; }
    }
}

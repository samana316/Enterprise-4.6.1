namespace Enterprise.Core.ObjectCreations
{
    public interface IFactory<TResult>
    {
        TResult Create();
    }

    public interface IFactory<TKey, TResult>
    {
        TResult Create(TKey key);
    }
}

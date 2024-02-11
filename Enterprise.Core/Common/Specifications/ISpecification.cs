namespace Enterprise.Core.Common.Specifications
{
    public interface ISpecification<in T>
    {
        bool Evaluate(T value);
    }
}

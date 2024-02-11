using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Specifications
{
    partial class Specification
    {
        public static ISpecification<T> Or<T>(
            this ISpecification<T> first,
            ISpecification<T> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new OrSpecification<T>(first, second);
        }

        private class OrSpecification<T> : CompositeSpecification<T>
        {
            internal OrSpecification(
                ISpecification<T> first,
                ISpecification<T> second)
                : base(first, second)
            {
            }

            public override bool Evaluate(
                T value)
            {
                return this.First.Evaluate(value) || this.Second.Evaluate(value);
            }
        }
    }
}

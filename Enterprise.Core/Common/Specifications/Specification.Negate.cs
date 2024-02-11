using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Specifications
{
    partial class Specification
    {
        public static ISpecification<T> Negate<T>(
            this ISpecification<T> specification)
        {
            Check.NotNull(specification, "specification");

            return new NotSpecification<T>(specification);
        }

        private class NotSpecification<T> : ISpecification<T>
        {
            private readonly ISpecification<T> specification;

            internal NotSpecification(
                ISpecification<T> specification)
            {
                this.specification = specification;
            }

            public bool Evaluate(
                T value)
            {
                return !this.specification.Evaluate(value);
            }
        }
    }
}

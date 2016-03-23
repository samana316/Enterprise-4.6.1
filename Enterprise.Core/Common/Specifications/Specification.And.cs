using System;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Specifications
{
    partial class Specification
    {
        public static ISpecification<T> And<T>(
            this ISpecification<T> first,
            ISpecification<T> second)
        {
            Check.NotNull(first, "first");
            Check.NotNull(second, "second");

            return new AndSpecification<T>(first, second);
        }

        private class AndSpecification<T> : CompositeSpecification<T>
        {
            internal AndSpecification(
                ISpecification<T> first,
                ISpecification<T> second)
                : base(first, second)
            {
            }

            public override bool Evaluate(
                T value)
            {
                return this.First.Evaluate(value) && this.Second.Evaluate(value);
            }
        }
    }
}

using System;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Specifications
{
    public static partial class Specification
    {
        public static ISpecification<T> AlwaysFalse<T>()
        {
            return AlwaysFalseSpecification<T>.Instance;
        }

        public static ISpecification<T> AlwaysTrue<T>()
        {
            return AlwaysTrueSpecification<T>.Instance;
        }

        public static ISpecification<T> Create<T>(
            Func<T, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return new AnonymousSpecification<T>(predicate);
        }

        private class AlwaysFalseSpecification<T> : ISpecification<T>
        {
            public static ISpecification<T> Instance = new AlwaysFalseSpecification<T>();

            private AlwaysFalseSpecification()
            {
            }

            public bool Evaluate(
                T value)
            {
                return false;
            }
        }

        private class AlwaysTrueSpecification<T> : ISpecification<T>
        {
            public static ISpecification<T> Instance = new AlwaysTrueSpecification<T>();

            private AlwaysTrueSpecification()
            {
            }

            public bool Evaluate(
                T value)
            {
                return true;
            }
        }

        private class AnonymousSpecification<T> : ISpecification<T>
        {
            private readonly Func<T, bool> predicate;

            public AnonymousSpecification(
                Func<T, bool> predicate)
            {
                this.predicate = predicate;
            }

            public bool Evaluate(
                T value)
            {
                return this.predicate(value);
            }
        }

        private abstract class CompositeSpecification<T> : ISpecification<T>
        {
            private readonly ISpecification<T> first;

            private readonly ISpecification<T> second;

            protected CompositeSpecification(
                ISpecification<T> first,
                ISpecification<T> second)
            {
                this.first = first;
                this.second = second;
            }

            protected virtual ISpecification<T> First
            {
                get { return this.first; }
            }

            protected virtual ISpecification<T> Second
            {
                get { return this.second; }
            }

            public abstract bool Evaluate(T value);
        }
    }
}

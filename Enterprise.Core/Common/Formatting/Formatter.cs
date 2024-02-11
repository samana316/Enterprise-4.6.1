using System;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Formatting
{
    public abstract class Formatter<T> : NotifyUnhandledException
    {
        public static Formatter<T> Default
        {
            get { return DefaultFormatter.Instance; }
        }

        public static Formatter<T> Create(
            Func<T, string> function)
        {
            Check.NotNull(function, "function");

            return new AnonymousFormatter(function);
        }

        public abstract string Format(T value);

        public virtual bool TryFormat(
            T value,
            out string result)
        {
            try
            {
                result = this.Format(value);

                return true;
            }
            catch (Exception exception)
            {
                this.OnUnhandledException(exception);
                result = string.Empty;

                return false;
            }
        }

        private class AnonymousFormatter : Formatter<T>
        {
            private readonly Func<T, string> function;

            public AnonymousFormatter(
                Func<T, string> function)
            {
                this.function = function;
            }

            public override string Format(
                T value)
            {
                return function(value);
            }
        }

        private class DefaultFormatter : Formatter<T>
        {
            public static readonly DefaultFormatter Instance = new DefaultFormatter();

            private DefaultFormatter()
            {
            }

            public override string Format(
                T value)
            {
                return Convert.ToString(value);
            }
        }
    }
}

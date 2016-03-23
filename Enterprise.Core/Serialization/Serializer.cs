using System;
using System.Collections.Generic;
using Enterprise.Core.Common.Formatting;

namespace Enterprise.Core.Serialization
{
    public abstract class Serializer : Formatter<object>
    {
        public T Clone<T>(
            object value)
        {
            return (T)this.Clone(value, typeof(T));
        }

        public object Clone(
            object value,
            Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (value == null)
            {
                return null;
            }

            var message = this.Serialize(value);

            return this.Deserialize(message, type);
        }

        public T Deserialize<T>(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentOutOfRangeException("message");
            }

            return (T)this.Deserialize(message, typeof(T));
        }

        public abstract object Deserialize(string message, Type type);

        public override sealed string Format(
            object value)
        {
            return this.Serialize(value);
        }

        public string Serialize<T>(T value)
        {
            var type = this.FindRuntimeType(value);

            return this.DoSerialize(value, type);
        }

        protected virtual Type FindRuntimeType<T>(
            T value)
        {
            var type = typeof(T);
            var comparer = EqualityComparer<object>.Default;

            if (!comparer.Equals(value, null))
            {
                type = value.GetType();
            }

            return type;
        }

        protected abstract string DoSerialize(object value, Type type);

        internal string SerializeInternal(
            object value,
            Type type)
        {
            return this.DoSerialize(value, type);
        }
    }
}

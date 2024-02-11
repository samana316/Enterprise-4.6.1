using System;

namespace Enterprise.Core.ObjectCreations
{
    public interface IPrototype<T> : ICloneable
    {
        new T Clone();
    }
}

using System;
using Stateflows.Common.Classes;

namespace Stateflows.Common
{
    [Obsolete("GlobalValue<T> is obsolete, use IValue<T> or target type directly with GlobalValueAttribute.")]
    public class GlobalValue<T> : BaseValueAccessor<T>
    {
        public GlobalValue(string valueName) : base(valueName, () => ContextValues.GlobalValuesHolder.Value, nameof(ContextValues.GlobalValuesHolder))
        { }
    }
}

using Stateflows.Common.Classes;

namespace Stateflows.Common
{
    public class GlobalValue<T> : BaseValueAccessor<T>
    {
        public GlobalValue(string valueName) : base(valueName, () => ContextValues.GlobalValuesHolder.Value, nameof(ContextValues.GlobalValuesHolder))
        { }
    }
}

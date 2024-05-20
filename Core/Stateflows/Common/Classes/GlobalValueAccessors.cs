using Stateflows.Common.Classes;

namespace Stateflows.Common
{
    public class GlobalValue<T> : BaseValueAccessor<T>
    {
        public GlobalValue(string valueName) : base(valueName, () => ValuesHolder.GlobalValues.Value, nameof(ValuesHolder.GlobalValues))
        { }
    }
}

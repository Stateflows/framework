using Stateflows.Common.Classes;

namespace Stateflows.StateMachines
{
    public class StateValue<T> : BaseValueAccessor<T>
    {
        public StateValue(string valueName) : base(valueName, () => ValuesHolder.StateValues.Value, nameof(ValuesHolder.StateValues))
        { }
    }

    public class SourceStateValue<T> : BaseValueAccessor<T>
    {
        public SourceStateValue(string valueName) : base(valueName, () => ValuesHolder.SourceStateValues.Value, nameof(ValuesHolder.SourceStateValues))
        { }
    }

    public class TargetStateValue<T> : BaseValueAccessor<T>
    {
        public TargetStateValue(string valueName) : base(valueName, () => ValuesHolder.TargetStateValues.Value, nameof(ValuesHolder.TargetStateValues))
        { }
    }
}

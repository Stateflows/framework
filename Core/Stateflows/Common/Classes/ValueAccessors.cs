using Stateflows.Common.Classes;

namespace Stateflows.StateMachines
{
    public class StateValue<T> : BaseValueAccessor<T>
    {
        public StateValue(string valueName) : base(valueName, () => ContextValuesHolder.StateValues.Value, nameof(ContextValuesHolder.StateValues))
        { }
    }

    public class SourceStateValue<T> : BaseValueAccessor<T>
    {
        public SourceStateValue(string valueName) : base(valueName, () => ContextValuesHolder.SourceStateValues.Value, nameof(ContextValuesHolder.SourceStateValues))
        { }
    }

    public class TargetStateValue<T> : BaseValueAccessor<T>
    {
        public TargetStateValue(string valueName) : base(valueName, () => ContextValuesHolder.TargetStateValues.Value, nameof(ContextValuesHolder.TargetStateValues))
        { }
    }
}

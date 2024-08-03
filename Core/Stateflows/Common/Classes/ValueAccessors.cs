using Stateflows.Common.Classes;

namespace Stateflows.StateMachines
{
    public class StateValue<T> : BaseValueAccessor<T>
    {
        public StateValue(string valueName) : base(valueName, () => ContextValues.StateValuesHolder.Value, nameof(ContextValues.StateValuesHolder))
        { }
    }

    public class SourceStateValue<T> : BaseValueAccessor<T>
    {
        public SourceStateValue(string valueName) : base(valueName, () => ContextValues.SourceStateValuesHolder.Value, nameof(ContextValues.SourceStateValuesHolder))
        { }
    }

    public class TargetStateValue<T> : BaseValueAccessor<T>
    {
        public TargetStateValue(string valueName) : base(valueName, () => ContextValues.TargetStateValuesHolder.Value, nameof(ContextValues.TargetStateValuesHolder))
        { }
    }
}

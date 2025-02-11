﻿using System;
using Stateflows.Common.Classes;

namespace Stateflows.StateMachines
{
    [Obsolete("StateValue<T> is obsolete, use IValue<T> or target type directly with StateValueAttribute.")]
    public class StateValue<T> : BaseValueAccessor<T>
    {
        public StateValue(string valueName) : base(valueName, () => ContextValues.StateValuesHolder.Value, nameof(ContextValues.StateValuesHolder))
        { }
    }

    [Obsolete("StateValueSet is obsolete, use IValueSet with StateValueSetAttribute.")]
    public class StateValueSet : BaseValueSetAccessor
    {
        public StateValueSet(string valueSetName) : base(valueSetName, () => ContextValues.StateValuesHolder.Value, nameof(ContextValues.StateValuesHolder))
        { }
    }

    [Obsolete("SourceStateValue<T> is obsolete, use IValue<T> or target type directly with SourceStateValueAttribute.")]
    public class SourceStateValue<T> : BaseValueAccessor<T>
    {
        public SourceStateValue(string valueName) : base(valueName, () => ContextValues.SourceStateValuesHolder.Value, nameof(ContextValues.SourceStateValuesHolder))
        { }
    }

    [Obsolete("SourceStateValueSet is obsolete, use IValueSet with SourceStateValueSetAttribute.")]
    public class SourceStateValueSet : BaseValueSetAccessor
    {
        public SourceStateValueSet(string valueSetName) : base(valueSetName, () => ContextValues.SourceStateValuesHolder.Value, nameof(ContextValues.SourceStateValuesHolder))
        { }
    }

    [Obsolete("TargetStateValue<T> is obsolete, use IValue<T> or target type directly with TargetStateValueAttribute.")]
    public class TargetStateValue<T> : BaseValueAccessor<T>
    {
        public TargetStateValue(string valueName) : base(valueName, () => ContextValues.TargetStateValuesHolder.Value, nameof(ContextValues.TargetStateValuesHolder))
        { }
    }

    [Obsolete("TargetStateValueSet is obsolete, use IValueSet with TargetStateValueSetAttribute.")]
    public class TargetStateValueSet : BaseValueSetAccessor
    {
        public TargetStateValueSet(string valueSetName) : base(valueSetName, () => ContextValues.TargetStateValuesHolder.Value, nameof(ContextValues.TargetStateValuesHolder))
        { }
    }
}

using System;
using System.Threading;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    internal static class ValuesHolder
    {
        public static readonly AsyncLocal<IContextValues> GlobalValues = new AsyncLocal<IContextValues>();
        public static readonly AsyncLocal<IContextValues> StateValues = new AsyncLocal<IContextValues>();
        public static readonly AsyncLocal<IContextValues> SourceStateValues = new AsyncLocal<IContextValues>();
        public static readonly AsyncLocal<IContextValues> TargetStateValues = new AsyncLocal<IContextValues>();
    }

    public class BaseValueAccessor<T>
    {
        private readonly string valueName;
        private readonly Func<IContextValues> valueSetSelector;

        public BaseValueAccessor(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            if (valueSetSelector == null)
            {
                throw new StateflowsException($"{collectionName} set is not available in current context");
            }

            this.valueName = valueName;
            this.valueSetSelector = valueSetSelector;
        }

        public bool IsSet
            => valueSetSelector().TryGet<T>(valueName, out var _);

        public T Value
        {
            get => valueSetSelector().TryGet<T>(valueName, out var result)
                ? result
                : default;

            set => valueSetSelector().Set(valueName, value);
        }
    }

    public class GlobalValue<T> : BaseValueAccessor<T>
    {
        public GlobalValue(string valueName) : base(valueName, () => ValuesHolder.GlobalValues.Value, nameof(ValuesHolder.GlobalValues))
        { }
    }

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

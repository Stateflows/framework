using System;
using System.Threading;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
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
        private readonly IContextValues valueSet;

        public BaseValueAccessor(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            this.valueName = valueName;
            this.valueSet = valueSetSelector?.Invoke() ?? throw new StateflowsException($"{collectionName} set is not available in current context");
        }

        public bool IsSet
            => valueSet.TryGet<T>(valueName, out var _);

        public T Value
        {
            get => valueSet.TryGet<T>(valueName, out var result)
                ? result
                : default;

            set => valueSet.Set(valueName, value);
        }
    }
}

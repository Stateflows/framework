using System;
using System.Threading;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    public static class ContextValuesHolder
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

        public void Set(T value)
            => valueSet.Set(valueName, value);

        public bool IsSet
            => valueSet.IsSet(valueName);

        public bool TryGet(out T value)
            => valueSet.TryGet(valueName, out value);

        public T GetOrDefault(T defaultValue)
            => valueSet.GetOrDefault(valueName, defaultValue);

        public void Remove()
            => valueSet.Remove(valueName);
    }
}

using System;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    public class BaseValueAccessor<T>
    {
        private readonly string valueName;
        private readonly IContextValues valueSet;

        public BaseValueAccessor(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            this.valueName = valueName;
            this.valueSet = valueSetSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }

        public void Set(T value)
            => valueSet.Set(valueName, value);

        public bool IsSet
            => valueSet.IsSet(valueName);

        public bool TryGet(out T value)
            => valueSet.TryGet(valueName, out value);

        public T GetOrDefault(T defaultValue = default)
            => valueSet.GetOrDefault(valueName, defaultValue);

        public void Update(Func<T, T> valueUpdater, T defaultValue = default)
            => valueSet.Update(valueName, valueUpdater, defaultValue);

        public void Remove()
            => valueSet.Remove(valueName);
    }
}

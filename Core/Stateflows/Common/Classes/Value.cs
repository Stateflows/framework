using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    internal class Value<T> : IValue<T>
    {
        private readonly string valueName;
        private readonly IContextValues valueSet;

        public Value(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            this.valueName = valueName;
            this.valueSet = valueSetSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }
        
        public Task SetAsync(T value)
            => valueSet.SetAsync(valueName, value);

        public Task<bool> IsSetAsync()
            => valueSet.IsSetAsync(valueName);

        public Task<(bool Success, T Value)> TryGetAsync()
            => valueSet.TryGetAsync<T>(valueName);

        public Task<T> GetOrDefaultAsync(T defaultValue = default)
            => valueSet.GetOrDefaultAsync(valueName, defaultValue);

        public Task UpdateAsync(Func<T, T> valueUpdater, T defaultValue = default)
            => valueSet.UpdateAsync(valueName, valueUpdater, defaultValue);

        public Task RemoveAsync()
            => valueSet.RemoveAsync(valueName);
    }
}

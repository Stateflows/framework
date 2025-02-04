using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    public class BaseValueSetAccessor
    {
        private readonly string valueSetName;
        private readonly IContextValues parentValueSet;

        protected BaseValueSetAccessor(string valueSetName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            this.valueSetName = valueSetName;
            this.parentValueSet = valueSetSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }
        
        public Task SetAsync<T>(string valueName, T value)
            => parentValueSet.SetAsync($"{valueSetName}.{valueName}", value);

        public Task<bool> IsSetAsync(string valueName)
            => parentValueSet.IsSetAsync($"{valueSetName}.{valueName}");

        public Task<(bool Success, T Value)> TryGetAsync<T>(string valueName)
            => parentValueSet.TryGetAsync<T>($"{valueSetName}.{valueName}");

        public Task<T> GetOrDefaultAsync<T>(string valueName, T defaultValue = default)
            => parentValueSet.GetOrDefaultAsync($"{valueSetName}.{valueName}", defaultValue);

        public Task UpdateAsync<T>(string valueName, Func<T, T> valueUpdater, T defaultValue = default)
            => parentValueSet.UpdateAsync($"{valueSetName}.{valueName}", valueUpdater, defaultValue);

        public Task RemoveAsync(string valueName)
            => parentValueSet.RemoveAsync($"{valueSetName}.{valueName}");
        
        public Task ClearAsync()
            => ((ContextValuesCollection)parentValueSet).RemoveMatchingAsync(new Regex($"{valueSetName}[.](.*)"));
    }
}

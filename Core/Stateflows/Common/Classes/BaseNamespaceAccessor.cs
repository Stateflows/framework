using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    public class BaseNamespaceAccessor : INamespace
    {
        private readonly string namespaceName;
        private readonly Func<IContextValues> namespaceSelector;
        private readonly string collectionName;
        private readonly IContextValues parentValueSet;

        protected BaseNamespaceAccessor(string namespaceName, Func<IContextValues> namespaceSelector, string collectionName)
        {
            this.namespaceName = namespaceName;
            this.namespaceSelector = namespaceSelector;
            this.collectionName = collectionName;
            this.parentValueSet = namespaceSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }
        
        public Task SetAsync<T>(string valueName, T value)
            => parentValueSet.SetAsync($"{namespaceName}.{valueName}", value);

        public Task<bool> IsSetAsync(string valueName)
            => parentValueSet.IsSetAsync($"{namespaceName}.{valueName}");

        public Task<(bool Success, T Value)> TryGetAsync<T>(string valueName)
            => parentValueSet.TryGetAsync<T>($"{namespaceName}.{valueName}");

        public Task<T> GetOrDefaultAsync<T>(string valueName, T defaultValue = default)
            => parentValueSet.GetOrDefaultAsync($"{namespaceName}.{valueName}", defaultValue);

        public Task UpdateAsync<T>(string valueName, Func<T, T> valueUpdater, T defaultValue = default)
            => parentValueSet.UpdateAsync($"{namespaceName}.{valueName}", valueUpdater, defaultValue);

        public Task RemoveAsync(string valueName)
            => parentValueSet.RemoveAsync($"{namespaceName}.{valueName}");

        public IValue<T> GetValue<T>(string key)
            => new Value<T>($"{this.namespaceName}.{key}", namespaceSelector, collectionName);

        public INamespace GetNamespace(string namespaceName)
            => new BaseNamespaceAccessor($"{this.namespaceName}.{namespaceName}", namespaceSelector, collectionName);

        public Task ClearAsync()
            => ((ContextValuesCollection)parentValueSet).RemoveMatchingAsync(new Regex($"{namespaceName}[.](.*)", RegexOptions.None, TimeSpan.FromSeconds(1)));
    }
}

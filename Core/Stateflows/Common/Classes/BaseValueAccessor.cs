using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common.Classes
{
    public class BaseValueAccessor<T>
    {
        private readonly string valueName;
        private readonly IContextValues valueSet;

        protected BaseValueAccessor(string valueName, Func<IContextValues> valueSetSelector, string collectionName)
        {
            this.valueName = valueName;
            this.valueSet = valueSetSelector?.Invoke() ?? throw new StateflowsDefinitionException($"{collectionName} set is not available in current context");
        }

        [Obsolete("Method Set() is deprecated and will be removed soon. Use SetAsync() instead")]
        public void Set(T value)
            => valueSet.Set(valueName, value);

        [Obsolete("Method IsSet() is deprecated and will be removed soon. Use IsSetAsync() instead")]
        public bool IsSet
            => valueSet.IsSet(valueName);

        [Obsolete("Method TryGet() is deprecated and will be removed soon. Use TryGetAsync() instead")]
        public bool TryGet(out T value)
            => valueSet.TryGet(valueName, out value);

        [Obsolete("Method GetOrDefault() is deprecated and will be removed soon. Use GetOrDefaultAsync() instead")]
        public T GetOrDefault(T defaultValue = default)
            => valueSet.GetOrDefault(valueName, defaultValue);

        [Obsolete("Method Update() is deprecated and will be removed soon. Use UpdateAsync() instead")]
        public void Update(Func<T, T> valueUpdater, T defaultValue = default)
            => valueSet.Update(valueName, valueUpdater, defaultValue);

        [Obsolete("Method Remove() is deprecated and will be removed soon. Use RemoveAsync() instead")]
        public void Remove()
            => valueSet.Remove(valueName);
        
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

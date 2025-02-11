using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IValue<T>
    {
        Task SetAsync(T value);

        Task<bool> IsSetAsync();

        Task<(bool Success, T Value)> TryGetAsync();

        Task<T> GetOrDefaultAsync(T defaultValue = default);

        Task UpdateAsync(Func<T, T> valueUpdater, T defaultValue = default);

        Task RemoveAsync();
    }
}

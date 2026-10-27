namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IValuesGrain")]
internal interface IValuesGrain : IGrainWithStringKey
{
    [Alias("SetAsync")]
    Task SetAsync(string key, string value);
    [Alias("IsSetAsync")]
    Task<bool> IsSetAsync(string key);
    [Alias("HasAnyPrefixedAsync")]
    Task<bool> HasAnyPrefixedAsync(string prefix);
    [Alias("TryGetAsync")]
    Task<(bool Success, string Value)> TryGetAsync<T>(string key);
    [Alias("GetOrDefaultAsync")]
    Task<string> GetOrDefaultAsync(string key, string? defaultValue = null);
    [Alias("RemoveAsync")]
    Task RemoveAsync(string key);
    [Alias("RemovePrefixedAsync")]
    Task RemovePrefixedAsync(string prefix);
    [Alias("ClearAsync")]
    Task ClearAsync();
}
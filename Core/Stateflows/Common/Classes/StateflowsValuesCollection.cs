using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes;

internal class StateflowsValuesCollection : IContextValues
{
    private readonly string Scope;
    
    public Dictionary<string, StateflowsValue> Values { get; }

    public StateflowsValuesCollection(Dictionary<string, StateflowsValue> values = null, string prefix = "")
    {
        Values = values;//?.ToDictionary() ?? new();
        Scope = prefix == string.Empty
            ? $"ValuesStorage.Global"
            : $"ValuesStorage.{prefix}";
    }

    private string GetKey(string key)
        => $"{Scope}.{key}";

    private void InternalSet<T>(string key, T value)
    {
        if (!Values.TryGetValue(key, out var stateflowsValue))
        {
            stateflowsValue = new StateflowsValue() { Name = key };
            Values[key] = stateflowsValue;
        }

        stateflowsValue.Set(value);
    }

    private T InternalGetOrDefault<T>(string key, T defaultValue)
    {
        var result = defaultValue;
        if (Values.TryGetValue(key, out var stateflowsValue))
        {
            stateflowsValue.TryGetAs(out result);
        }

        return result;
    }

    private bool InternalTryGet<T>(string key, out T value)
    {
        value = default;
        return Values.TryGetValue(key, out var stateflowsValue) && stateflowsValue.TryGetAs(out value);
    }

    private void InternalRemove(string key)
    {
        if (Values.TryGetValue(key, out var stateflowsValue))
        {
            stateflowsValue.Remove();
        }
    }

    private void InternalRemovePrefixed(string prefix)
    {
        foreach (var stateflowsValue in Values.Values.Where(v => v.Name.StartsWith($"{Scope}.{prefix}.")))
        {
            stateflowsValue.Remove();
        }
    }

    public Task SetAsync<T>(string key, T value)
    {
        key = GetKey(key);
        lock (Values)
        {
            InternalSet(key, value);
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsSetAsync(string key)
    {
        key = GetKey(key);
        bool result;

        lock (Values)
        {
            result = Values.ContainsKey(key);
        }

        return Task.FromResult(result);
    }
    
    public Task<(bool Success, T Value)> TryGetAsync<T>(string key)
    {
        key = GetKey(key);
        (bool Success, T Value) result = (false, default);

        lock (Values)
        {
            result.Success = InternalTryGet(key, out result.Value);
        }

        return Task.FromResult(result);
    }
    
    public Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default)
    {
        key = GetKey(key);
        lock (Values)
        {
            return Task.FromResult(InternalGetOrDefault(key, defaultValue));
        }
    }

    public Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default)
    {
        key = GetKey(key);
        lock (Values)
        {
            var value = InternalGetOrDefault(key, defaultValue);

            value = valueUpdater(value);

            InternalSet(key, value);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        key = GetKey(key);
        lock (Values)
        {
            InternalRemove(key);
        }

        return Task.CompletedTask;
    }

    public Task RemovePrefixedAsync(string prefix)
    {
        prefix = GetKey(prefix);
        lock (Values)
        {
            InternalRemovePrefixed(prefix);
        }

        return Task.CompletedTask;
    }

    public Task<bool> HasAnyPrefixedAsync(string prefix)
    {
        prefix = GetKey(prefix);
        lock (Values)
        {
            return Task.FromResult(Values.Keys.Any(key => key.StartsWith($"{Scope}.{prefix}.")));
        }
    }

    public Task ClearAsync()
    {
        lock (Values)
        {
            if (Scope == "ValuesStorage.Global")
            {
                foreach (var stateflowsValue in Values.Values)
                {
                    stateflowsValue.Remove();
                }
            }
            else
            {
                InternalRemovePrefixed(Scope);
            }
        }

        return Task.CompletedTask;
    }
}

using System.Collections.Generic;

namespace Stateflows.Common;

/// <summary>Provides bulk insertion into dictionaries.</summary>
public static class DictionaryExtensions
{
    /// <summary>Adds each supplied entry using normal dictionary insertion semantics.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary receiving entries.</param>
    /// <param name="items">The entries to add in enumeration order.</param>
    /// <remarks>Duplicate keys throw; entries added before the duplicate remain in the dictionary.</remarks>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
        IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var item in items)
        {
            dictionary.Add(item.Key, item.Value);
        }
    }
}
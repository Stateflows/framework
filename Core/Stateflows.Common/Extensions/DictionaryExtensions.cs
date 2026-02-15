using System.Collections.Generic;

namespace Stateflows.Common;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
        IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var item in items)
        {
            dictionary.Add(item.Key, item.Value);
        }
    }
}
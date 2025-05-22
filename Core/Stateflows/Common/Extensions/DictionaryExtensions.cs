using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Stateflows.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void RemoveMatching<TValue>(this Dictionary<string, TValue> dictionary, Regex keyPattern)
        {
            var matchingKeys = dictionary.Keys.Where(key => keyPattern.IsMatch(key));
            foreach (var key in matchingKeys)
            {
                dictionary.Remove(key);
            }
        }
    }
}
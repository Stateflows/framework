using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;

namespace Stateflows.Payloads
{
    /// <summary>Validates and normalizes sets of payload names used by event and token declarations.</summary>
    internal static class PayloadNameSet
    {
        private const char Separator = '|';

        /// <summary>Validates rule V1: non-empty set, no empty or whitespace entries, no duplicates.</summary>
        public static IReadOnlyList<string> Validate(IEnumerable<string> names, string paramName)
        {
            if (names == null) throw new ArgumentNullException(paramName);
            var list = names.ToArray();
            if (list.Length == 0)
                throw new ArgumentException("At least one payload name is required.", paramName);
            if (list.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Payload names cannot be null, empty or whitespace.", paramName);
            if (list.Distinct(StringComparer.Ordinal).Count() != list.Length)
                throw new ArgumentException("Payload names must be distinct.", paramName);
            return list;
        }

        /// <summary>Order-insensitive identity of a name set; equals the name itself for a single-element set.</summary>
        public static string Key(IEnumerable<string> names)
            => string.Join(Separator.ToString(), names.OrderBy(name => name, StringComparer.Ordinal));

        public static bool SetEquals(IEnumerable<string> first, IEnumerable<string> second)
            => first != null && second != null && new HashSet<string>(first, StringComparer.Ordinal).SetEquals(second);

        /// <summary>True for CLR types that represent payloads and therefore can only be selected by name (rule V4).</summary>
        public static bool IsPayloadType(Type type)
            => type != null && typeof(PayloadValue).IsAssignableFrom(type);
    }
}

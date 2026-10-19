using System;

namespace Stateflows.Common.Utilities
{
    internal static class UtcTime
    {
        /// <summary>
        /// Time events are scheduled in UTC. Local values are converted; unspecified values are taken as UTC.
        /// </summary>
        public static DateTime Normalize(DateTime value)
            => value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            };

        public static DateTime? Normalize(DateTime? value)
            => value.HasValue ? Normalize(value.Value) : (DateTime?)null;
    }
}

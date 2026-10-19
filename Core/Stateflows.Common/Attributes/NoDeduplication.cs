using System;

namespace Stateflows.Common
{
    /// <summary>
    /// Events of a type marked with this attribute are not deduplicated by event id: a redelivery executes again
    /// and no receipt is recorded. Use only for idempotent events, such as read-only requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoDeduplicationAttribute : Attribute
    { }
}

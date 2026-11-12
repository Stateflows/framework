using System;

namespace Stateflows.Common
{
    /// <summary>Marks a notification type whose notifications are retained by the notification subscriber.</summary>
    /// <remarks>
    /// Only the latest retained notification of a type remains available for queries and subscription retries.
    /// A positive time-to-live expires it; zero keeps it indefinitely.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class RetainAttribute : Attribute
    { }
}

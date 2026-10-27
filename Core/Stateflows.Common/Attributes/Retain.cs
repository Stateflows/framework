using System;

namespace Stateflows.Common
{
    /// <summary>Marks a notification type whose notifications are retained by the notification subscriber.</summary>
    /// <remarks>Retained notifications remain available for later retrieval through notification queries.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class RetainAttribute : Attribute
    { }
}

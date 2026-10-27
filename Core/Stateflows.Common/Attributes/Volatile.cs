using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event payload type as volatile.</summary>
    /// <remarks>This is a marker for integrations that distinguish transient events from durably retained events.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class VolatileAttribute : Attribute
    { }
}

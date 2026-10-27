using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event type whose events must not be forwarded to other behaviors.</summary>
    /// <remarks>This marker is applied to the event payload type and is evaluated by behavior execution pipelines.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoForwardingAttribute : Attribute
    { }
}

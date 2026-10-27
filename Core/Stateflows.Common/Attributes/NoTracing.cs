using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event type to be omitted from event tracing.</summary>
    /// <remarks>Tracing integrations may use this marker to keep routine or sensitive events out of traces.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoTracingAttribute : Attribute
    { }
}

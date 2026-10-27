using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event payload type for strict-ownership handling.</summary>
    /// <remarks>This is a marker for integrations that enforce ownership constraints on event delivery.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class StrictOwnershipAttribute : Attribute
    { }
}

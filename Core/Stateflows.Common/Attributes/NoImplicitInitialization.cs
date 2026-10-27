using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event type that must not implicitly initialize its target behavior.</summary>
    /// <remarks>The event may still be processed when the behavior has already been initialized.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoImplicitInitializationAttribute : Attribute
    { }
}

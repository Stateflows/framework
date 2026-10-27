using System;

namespace Stateflows.Common
{
    /// <summary>Marks an event type whose events must not bubble to a containing behavior.</summary>
    /// <remarks>Use this marker when an event should be handled only by the behavior that directly receives it.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoBubblingAttribute : Attribute
    { }
}

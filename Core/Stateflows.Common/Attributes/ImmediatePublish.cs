using System;

namespace Stateflows.Common
{
    /// <summary>
    /// Publishes notifications of this type during execution instead of after publisher state commits.
    /// Notifications remain visible even if the publisher's execution or persistence later fails.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class ImmediatePublishAttribute : Attribute
    { }
}

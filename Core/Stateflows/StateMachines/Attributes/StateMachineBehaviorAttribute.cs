using System;
using Stateflows.Common.Attributes;

namespace Stateflows.StateMachines.Attributes
{
    /// <summary>Specifies the registered name, version, and resource name of a typed machine definition.</summary>
    /// <param name="name">The behavior class name, or null to use the type's readable name.</param>
    /// <param name="version">The definition version used during assembly discovery.</param>
    /// <param name="resourceName">Optional resource identifier associated with the definition.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class StateMachineBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

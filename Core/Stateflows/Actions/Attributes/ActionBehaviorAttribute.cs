using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Actions.Attributes
{
    /// <summary>Assigns action registration metadata to an action implementation type.</summary>
    /// <param name="name">Optional registered name; when omitted, the readable type name is used.</param>
    /// <param name="version">The action definition version.</param>
    /// <param name="resourceName">Optional resource name associated with the action.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

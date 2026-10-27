using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Activities.Attributes
{
    /// <summary>Assigns activity registration metadata to an activity implementation type.</summary>
    /// <param name="name">Optional registered name; when omitted, the readable type name is used.</param>
    /// <param name="version">The activity definition version.</param>
    /// <param name="resourceName">Optional resource name associated with the activity.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActivityBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

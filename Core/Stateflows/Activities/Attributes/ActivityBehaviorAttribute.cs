using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActivityBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

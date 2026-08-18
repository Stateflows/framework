using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Actions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

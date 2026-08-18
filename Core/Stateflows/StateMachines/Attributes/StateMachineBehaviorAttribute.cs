using System;
using Stateflows.Common.Attributes;

namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StateMachineBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}


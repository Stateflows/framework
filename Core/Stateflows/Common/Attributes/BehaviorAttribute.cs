using System;

namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class BehaviorAttribute(string? name = null, int version = 1, string? resourceName = null) : Attribute
    {
        public string? Name { get; init; } = name;

        public int Version { get; init; } = version;

        public string? ResourceName { get; init; } = resourceName;
    }
}

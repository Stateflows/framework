using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    public enum EntityScope
    {
        Self,
        Parent,
        Owner
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromProjectionAttribute(EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        public EntityScope Scope { get; init; } = scope;
        public bool Required { get; init; } = required;
    }
}

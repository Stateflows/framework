using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromFieldAttribute(string? name = null, EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        public string? Name { get; init; } = name;
        public EntityScope Scope { get; init; } = scope;
        public bool Required { get; init; } = required;
    }
}

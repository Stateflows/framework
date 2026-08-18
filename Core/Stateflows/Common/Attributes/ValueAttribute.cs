using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    public abstract class ValueAttribute(string? name = null, bool required = true) : Attribute
    {
        public string? Name { get; init; } = name;
        public bool Required { get; init; } = required;
    }
}

using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    public abstract class ValueAttribute : Attribute
    {
        public string? Name { get; set; }
        public bool Required { get; set; }

        public ValueAttribute(string? name = null, bool required = true)
        {
            Name = name;
            Required = required;
        }
    }
}

using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    public abstract class ValueSetAttribute : Attribute
    {
        public string? Name { get; set; }

        public ValueSetAttribute(string? name = null)
        {
            Name = name;
        }
    }
}

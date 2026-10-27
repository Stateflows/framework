using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Marks a parameter for injection as a set of named behavior values.</summary>
    /// <remarks>Derived attributes select the value-set scope; an optional name selects a named set.</remarks>
    public abstract class ValueSetAttribute : Attribute
    {
        /// <summary>Gets or sets the name identifying the requested value set.</summary>
        public string? Name { get; set; }

        /// <summary>Creates a value-set injection attribute.</summary>
        /// <param name="name">Optional name identifying the value set.</param>
        public ValueSetAttribute(string? name = null)
        {
            Name = name;
        }
    }
}

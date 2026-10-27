using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Marks an operation parameter for injection from a behavior value store.</summary>
    /// <remarks>The derived attribute selects a value scope; <see cref="Name"/> can override the parameter name and <see cref="Required"/> controls missing-value handling.</remarks>
    /// <param name="name">Optional value key; when omitted, the parameter name is used.</param>
    /// <param name="required">Whether resolution must find a value.</param>
    public abstract class ValueAttribute(string? name = null, bool required = true) : Attribute
    {
        /// <summary>Gets the value key to resolve, or <see langword="null"/> to use the parameter name.</summary>
        public string? Name { get; init; } = name;
        /// <summary>Gets whether the requested value must be available.</summary>
        public bool Required { get; init; } = required;
    }
}

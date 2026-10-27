using System;

namespace Stateflows.Entities.Attributes;

/// <summary>Defines which operations may read or write a registered entity field.</summary>
[Flags]
public enum FieldAccess
{
    /// <summary>The field is neither readable nor writable through the entity API.</summary>
    None = 0,
    /// <summary>The field can be set by callers.</summary>
    Set = 1,
    /// <summary>The field can be read by callers.</summary>
    Get = 2
}

/// <summary>Declares field access on an entity template property.</summary>
/// <param name="fieldAccess">The read and write access flags.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldAttribute(FieldAccess fieldAccess = FieldAccess.None) : Attribute
{
    /// <summary>Gets the configured access flags.</summary>
    public FieldAccess Access { get; init; } = fieldAccess;
}
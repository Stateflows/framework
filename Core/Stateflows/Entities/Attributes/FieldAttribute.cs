using System;

namespace Stateflows.Entities.Attributes;

[Flags]
public enum FieldAccess
{
    None = 0,
    Set = 1,
    Get = 2
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldAttribute(FieldAccess fieldAccess = FieldAccess.None) : Attribute
{
    public FieldAccess Access { get; init; } = fieldAccess;
}
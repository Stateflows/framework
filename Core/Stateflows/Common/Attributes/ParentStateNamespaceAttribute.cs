using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    /// <summary>Declares a named value namespace associated with the parent state.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    /// <remarks>This metadata marker is not currently resolved by the built-in parameter activator's namespace branches; it does not by itself enable parent-state namespace injection.</remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ParentStateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    /// <summary>Explicit-source alias for <see cref="ParentStateNamespaceAttribute"/>, with the same activation limitation.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    public sealed class FromParentStateNamespaceAttribute(string? name = null) : ParentStateNamespaceAttribute(name);
}

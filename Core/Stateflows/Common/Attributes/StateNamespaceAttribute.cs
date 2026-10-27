using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    /// <summary>Injects a named namespace over the current state's values into an <see cref="Stateflows.Common.INamespace"/> parameter.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class StateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    /// <summary>Explicit-source alias for <see cref="StateNamespaceAttribute"/>.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    public sealed class FromStateNamespaceAttribute(string? name = null) : StateNamespaceAttribute(name);
}

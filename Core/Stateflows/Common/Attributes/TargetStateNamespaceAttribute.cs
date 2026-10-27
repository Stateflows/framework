using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    /// <summary>Injects a named namespace over the transition target state's values into an <see cref="Stateflows.Common.INamespace"/> parameter.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetStateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    /// <summary>Explicit-source alias for <see cref="TargetStateNamespaceAttribute"/>.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    public sealed class FromTargetStateNamespaceAttribute(string? name = null) : TargetStateNamespaceAttribute(name);
}

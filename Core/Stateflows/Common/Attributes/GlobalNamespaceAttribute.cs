using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Injects a named namespace over the current behavior's global values into an <see cref="INamespace"/> parameter.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GlobalNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    /// <summary>Explicit-source alias for <see cref="GlobalNamespaceAttribute"/> with the same global-value namespace resolution.</summary>
    /// <param name="name">The namespace name, or <see langword="null"/> to use the parameter name.</param>
    public sealed class FromGlobalNamespaceAttribute(string? name = null) : GlobalNamespaceAttribute(name);
}

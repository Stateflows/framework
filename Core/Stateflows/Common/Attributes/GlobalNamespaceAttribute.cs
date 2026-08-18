using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GlobalNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    public sealed class FromGlobalNamespaceAttribute(string? name = null) : GlobalNamespaceAttribute(name);
}

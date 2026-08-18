using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class SourceStateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    public sealed class FromSourceStateNamespaceAttribute(string? name = null) : SourceStateNamespaceAttribute(name);
}

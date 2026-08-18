using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetStateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    public sealed class FromTargetStateNamespaceAttribute(string? name = null) : TargetStateNamespaceAttribute(name);
}

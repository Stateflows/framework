using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ParentStateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    public sealed class FromParentStateNamespaceAttribute(string? name = null) : ParentStateNamespaceAttribute(name);
}

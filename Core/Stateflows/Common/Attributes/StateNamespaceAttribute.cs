using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class StateNamespaceAttribute(string? name = null) : ValueSetAttribute(name);

    public sealed class FromStateNamespaceAttribute(string? name = null) : StateNamespaceAttribute(name);
}

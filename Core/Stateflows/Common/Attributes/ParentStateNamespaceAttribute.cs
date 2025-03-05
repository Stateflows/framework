using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ParentStateNamespaceAttribute : ValueSetAttribute
    {
        public ParentStateNamespaceAttribute(string? name = null) : base(name)
        { }
    }
}

using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class SourceStateNamespaceAttribute : ValueSetAttribute
    {
        public SourceStateNamespaceAttribute(string? name = null) : base(name)
        { }
    }
}

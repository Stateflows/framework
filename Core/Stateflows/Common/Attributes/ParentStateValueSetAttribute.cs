using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ParentStateValueSetAttribute : ValueSetAttribute
    {
        public ParentStateValueSetAttribute(string? name = null) : base(name)
        { }
    }
}

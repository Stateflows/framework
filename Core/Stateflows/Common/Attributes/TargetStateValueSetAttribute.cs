using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class TargetStateValueSetAttribute : ValueSetAttribute
    {
        public TargetStateValueSetAttribute(string? name = null) : base(name)
        { }
    }
}

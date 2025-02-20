using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class TargetStateValueAttribute : ValueAttribute
    {
        public TargetStateValueAttribute(string? name = null, bool required = true) : base(name, required)
        { }
    }
}

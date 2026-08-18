using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetStateValueAttribute(string? name = null, bool required = true) : ValueAttribute(name, required);

    public sealed class FromTargetStateValueAttribute(string? name = null, bool required = true) : TargetStateValueAttribute(name, required);
}

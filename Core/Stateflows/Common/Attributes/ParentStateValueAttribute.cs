using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ParentStateValueAttribute(string? name = null, bool required = true) : ValueAttribute(name, required);

    public sealed class FromParentStateValueAttribute(string? name = null, bool required = true) : ParentStateValueAttribute(name, required);
}

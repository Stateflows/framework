using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class StateValueAttribute(string? name = null, bool required = true) : ValueAttribute(name, required);

    public sealed class FromStateValueAttribute(string? name = null, bool required = true) : StateValueAttribute(name, required);
}

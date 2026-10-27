using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    /// <summary>Resolves an operation parameter from the transition target state's value collection.</summary>
    /// <param name="name">The value key, or <see langword="null"/> to use the parameter name.</param>
    /// <param name="required">Whether resolution must find the value rather than use the parameter's fallback.</param>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetStateValueAttribute(string? name = null, bool required = true) : ValueAttribute(name, required);

    /// <summary>Explicit-source alias for <see cref="TargetStateValueAttribute"/>.</summary>
    /// <param name="name">The value key, or <see langword="null"/> to use the parameter name.</param>
    /// <param name="required">Whether the value must be available.</param>
    public sealed class FromTargetStateValueAttribute(string? name = null, bool required = true) : TargetStateValueAttribute(name, required);
}

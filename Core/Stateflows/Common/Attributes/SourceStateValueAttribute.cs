using System;
using Stateflows.Common.Attributes;

#nullable enable
namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class SourceStateValueAttribute(string? name = null, bool required = true) : ValueAttribute(name, required);

    public sealed class FromSourceStateValueAttribute(string? name = null, bool required = true) : SourceStateValueAttribute(name, required);
}

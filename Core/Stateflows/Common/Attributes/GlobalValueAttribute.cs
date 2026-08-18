using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GlobalValueAttribute(string? name = null, bool required = true) :
        ValueAttribute(name, required);

    public sealed class FromGlobalValueAttribute(string? name = null, bool required = true) : GlobalValueAttribute(name, required);
}

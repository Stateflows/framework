using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class GlobalValueAttribute : ValueAttribute
    {
        public GlobalValueAttribute(string? name = null, bool required = true) : base(name, required)
        { }
    }
}

using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class GlobalValueSetAttribute : ValueSetAttribute
    {
        public GlobalValueSetAttribute(string? name = null) : base(name)
        { }
    }
}

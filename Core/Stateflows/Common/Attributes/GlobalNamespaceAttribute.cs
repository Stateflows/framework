using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class GlobalNamespaceAttribute : ValueSetAttribute
    {
        public GlobalNamespaceAttribute(string? name = null) : base(name)
        { }
    }
}

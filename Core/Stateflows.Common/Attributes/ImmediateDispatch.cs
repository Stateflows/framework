using System;

namespace Stateflows.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    internal sealed class ImmediateDispatchAttribute : Attribute
    { }
}

using System;

namespace Stateflows.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class EventAttribute : Attribute
    {
        public string Name { get; set; }

        public EventAttribute(string name)
        {
            Name = name;
        }
    }
}

using System;

namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StateMachineAttribute : Attribute
    {
        public string Name { get; }

        public int Version { get; }

        public StateMachineAttribute(string name = null, int version = 1)
        {
            Name = name;
            Version = version;
        }
    }
}

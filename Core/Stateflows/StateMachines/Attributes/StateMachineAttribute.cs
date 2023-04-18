using System;

namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StateMachineAttribute : Attribute
    {
        public string Name { get; set; }

        public StateMachineAttribute(string name)
        {
            Name = name;
        }

        public StateMachineAttribute()
        {
        }
    }
}

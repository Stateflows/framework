using System;

namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ValueSetNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ValueSetNameAttribute(string name)
        {
            Name = name;
        }
    }
}

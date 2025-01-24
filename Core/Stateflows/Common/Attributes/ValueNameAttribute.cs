using System;

namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ValueNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ValueNameAttribute(string name)
        {
            Name = name;
        }
    }
}

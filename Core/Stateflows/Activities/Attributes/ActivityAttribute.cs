using System;

namespace Stateflows.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActivityAttribute : Attribute
    {
        public string Name { get; set; }

        public ActivityAttribute(string name)
        {
            Name = name;
        }

        public ActivityAttribute()
        {
        }
    }
}

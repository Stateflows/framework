using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActivityBehaviorAttribute : BehaviorAttribute
    {
        public ActivityBehaviorAttribute(string name = null, int version = 1) : base(name, version)
        { }
    }
}

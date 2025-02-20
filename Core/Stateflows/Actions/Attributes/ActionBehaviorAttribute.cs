using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Actions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionBehaviorAttribute : BehaviorAttribute
    {
        public ActionBehaviorAttribute(string name = null, int version = 1) : base(name, version)
        { }
    }
}

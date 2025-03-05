using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Actions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionBehaviorAttribute : BehaviorAttribute
    {
        public bool Reentrant { get; private set; }

        public ActionBehaviorAttribute(string name = null, int version = 1, bool reentrant = true) : base(name, version)
        {
            Reentrant = reentrant;
        }
    }
}

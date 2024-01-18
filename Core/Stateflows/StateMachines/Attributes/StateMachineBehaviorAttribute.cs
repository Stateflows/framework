using System;
using Stateflows.Common.Attributes;

namespace Stateflows.StateMachines.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StateMachineBehaviorAttribute : BehaviorAttribute
    {
        public StateMachineBehaviorAttribute(string name = null, int version = 1) : base(name, version)
        { }
    }
}

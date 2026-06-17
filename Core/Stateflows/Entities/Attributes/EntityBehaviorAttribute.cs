using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityBehaviorAttribute : BehaviorAttribute
    {
        public EntityBehaviorAttribute(string name = null, int version = 1) : base(name, version)
        { }
    }
}


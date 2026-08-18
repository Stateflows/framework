using System;

namespace Stateflows.Activities.Exceptions
{
    internal class ActivityOverrideException : ActivityDefinitionException
    {
        public new ActivityClass ActivityClass { get; }

        public ActivityOverrideException(string message, ActivityClass activityClass) : base(message, activityClass.BehaviorClass)
        {
            ActivityClass = activityClass;
        }

        public ActivityOverrideException(string message, ActivityClass activityClass, Exception innerException) : base(message, activityClass.BehaviorClass, innerException)
        {
            ActivityClass = activityClass;
        }
    }
}

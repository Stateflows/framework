using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.Activities.Exceptions
{
    public class ActivityDefinitionException : BehaviorDefinitionException
    {
        public ActivityClass ActivityClass { get; }

        public ActivityDefinitionException(string message, ActivityClass activityClass) : base(message, activityClass)
        {
            ActivityClass = activityClass;
        }

        public ActivityDefinitionException(string message, ActivityClass activityClass, Exception innerException) : base(message, activityClass.BehaviorClass, innerException)
        {
            ActivityClass = activityClass;
        }
    }
}

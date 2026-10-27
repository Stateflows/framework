using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.Activities.Exceptions
{
    /// <summary>Reports an invalid or inconsistent activity definition.</summary>
    /// <remarks>The exception identifies the activity class whose definition failed validation.</remarks>
    public class ActivityDefinitionException : BehaviorDefinitionException
    {
        /// <summary>Gets the activity class whose definition failed validation.</summary>
        public ActivityClass ActivityClass { get; }

        /// <summary>Initializes an activity definition exception.</summary>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="activityClass">The activity class whose definition is invalid.</param>
        public ActivityDefinitionException(string message, ActivityClass activityClass) : base(message, activityClass)
        {
            ActivityClass = activityClass;
        }

        /// <summary>Initializes an activity definition exception with its underlying cause.</summary>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="activityClass">The activity class whose definition is invalid.</param>
        /// <param name="innerException">The exception that caused this definition error.</param>
        public ActivityDefinitionException(string message, ActivityClass activityClass, Exception innerException) : base(message, activityClass.BehaviorClass, innerException)
        {
            ActivityClass = activityClass;
        }
    }
}

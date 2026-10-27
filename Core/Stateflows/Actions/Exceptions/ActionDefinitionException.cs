using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.Actions.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an invalid or inconsistent action definition.</summary>
    /// <remarks>This exception identifies the affected action class and is raised while an action definition is being validated.</remarks>
    public class ActionDefinitionException : BehaviorDefinitionException
    {
        /// <summary>Gets the action class whose definition failed validation.</summary>
        public ActionClass ActionClass { get; }

        /// <summary>Initializes an action definition exception.</summary>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="actionClass">The action class whose definition is invalid.</param>
        public ActionDefinitionException(string message, ActionClass actionClass) : base(message, actionClass)
        {
            ActionClass = actionClass;
        }

        /// <summary>Initializes an action definition exception with its underlying cause.</summary>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="actionClass">The action class whose definition is invalid.</param>
        /// <param name="innerException">The exception that caused this definition error.</param>
        public ActionDefinitionException(string message, ActionClass actionClass, Exception innerException) : base(message, actionClass.BehaviorClass, innerException)
        {
            ActionClass = actionClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}

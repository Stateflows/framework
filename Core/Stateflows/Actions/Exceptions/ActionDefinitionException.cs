using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.Actions.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ActionDefinitionException : BehaviorDefinitionException
    {
        public ActionClass ActionClass { get; }

        public ActionDefinitionException(string message, ActionClass actionClass) : base(message, actionClass)
        {
            ActionClass = actionClass;
        }

        public ActionDefinitionException(string message, ActionClass actionClass, Exception innerException) : base(message, actionClass.BehaviorClass, innerException)
        {
            ActionClass = actionClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}

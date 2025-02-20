using Stateflows.Common.Exceptions;

namespace Stateflows.Actions.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ActionException : StateflowsDefinitionException
    {
        public ActionException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}

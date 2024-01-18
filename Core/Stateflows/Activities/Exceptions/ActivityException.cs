using Stateflows.Common.Exceptions;

namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ActivityException : StateflowsException
    {
        public ActivityException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}

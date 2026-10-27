using Stateflows.Common.Exceptions;

namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an activity-related definition error not associated with a specific activity class.</summary>
    public class ActivityException : StateflowsDefinitionException
    {
        /// <summary>Initializes an activity exception.</summary>
        /// <param name="message">Description of the error.</param>
        public ActivityException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
